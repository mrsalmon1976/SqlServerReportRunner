using NSubstitute;
using NUnit.Framework;
using OfficeOpenXml;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Writers;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Test.SqlServerReportRunner.Reporting.Writers
{
    [TestFixture]
    public class ExcelReportWriterTest
    {
        private IReportWriter _reportWriter;
        private string _testRootFolder = String.Empty;
        private string _filePath = String.Empty;
        private IExcelRangeFormatter _excelRangeFormatter;

        [SetUp]
        public void ExcelReportWriterTest_SetUp()
        {
            // create the root folder
            _testRootFolder = Path.Combine(Environment.CurrentDirectory, "ExcelReportWriterTest");
            Directory.CreateDirectory(_testRootFolder);

            _excelRangeFormatter = Substitute.For<IExcelRangeFormatter>();
            _excelRangeFormatter.When(x => x.FormatCell(Arg.Any<ExcelRange>(), Arg.Any<object>(), Arg.Any<Type>())).Do((r) => 
            {
                ExcelRange range = r.ArgAt<ExcelRange>(0);
                range.Value = r.ArgAt<object>(1);
            });
            _filePath = Path.Combine(_testRootFolder, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".xlsx");
            _reportWriter = new ExcelReportWriter(_excelRangeFormatter, _filePath);

        }

        [TearDown]
        public void ExcelReportWriterTest_TearDown()
        {
            Directory.Delete(_testRootFolder, true);
        }

        [Test]
        public void ExcelReportWriterTest_CheckFileContents()
        {
            const string delimiter = ",";
            // set up the data reader to return columns
            ColumnMetaData[] headers = {
                new ColumnMetaData("Name", "varchar", 100),
                new ColumnMetaData("Surname", "varchar", 100),
                new ColumnMetaData("Age", "varchar", 32),
            };
            object[][] data =
            {
                new object[] { "Matt", "Salmon", 41 },
                new object[] { "John", "Doe", 61 },
                new object[] { "Jane", "Smith", 25 }
            };

            IDataReader reader = Substitute.For<IDataReader>();
            reader.FieldCount.Returns(3);
            reader.Read().Returns(true, true, true, false);
            reader.GetValue(0).Returns(data[0][0], data[1][0], data[2][0]);
            reader.GetValue(1).Returns(data[0][1], data[1][1], data[2][1]);
            reader.GetValue(2).Returns(data[0][2], data[1][2], data[2][2]);

            // execute
            _reportWriter.Initialise();
            _reportWriter.WriteHeader(headers.Select(x => x.Name), delimiter);
            foreach (object[] line in data)
            {
                _reportWriter.WriteLine(reader, headers, delimiter);
            }
            _reportWriter.Dispose();

            // assert
            Assert.IsTrue(File.Exists(_filePath));
            List<string[]> lines = ReadSpreadsheetLines(_filePath);

            // check that the header is correct
            string expectedHeader = String.Join(delimiter, headers.Select(x => x.Name));
            Assert.AreEqual(expectedHeader, String.Join(delimiter, lines[0]));

            // make sure each of the lines is written correctly
            for (int i=0; i<data.Length; i++)
            {
                object[] line = data[i];
                string expectedLine = String.Join(delimiter, data[i]);
                string actualLine = String.Join(delimiter, lines[i + 1]);
                Assert.AreEqual(expectedLine, actualLine);

            }

            _excelRangeFormatter.Received(data.Length * data[0].Length).FormatCell(Arg.Any<ExcelRange>(), Arg.Any<object>(), Arg.Any<Type>());
        }

        [Test]
        public void ExcelReportWriterTest_DataContainsNulls_EmptyStringReturned()
        {
            const string delimiter = ",";

            // set up the data reader to return columns
            ColumnMetaData[] headers = {
                new ColumnMetaData("Name", "varchar", 100),
                new ColumnMetaData("Surname", "varchar", 100)
            };
            object[][] data =
            {
                new object[] { "Valid", "valid" },
                new object[] { "DbNull", DBNull.Value },
                new object[] { "null", null  }
            };

            IDataReader reader = Substitute.For<IDataReader>();
            reader.FieldCount.Returns(2);
            reader.Read().Returns(true, true, true, false);
            reader.GetValue(0).Returns(data[0][0], data[1][0], data[2][0]);
            reader.GetValue(1).Returns(data[0][1], data[1][1], data[2][1]);

            // execute
            _reportWriter.Initialise();
            foreach (object[] line in data)
            {
                _reportWriter.WriteLine(reader, headers, delimiter);
            }
            _reportWriter.Dispose();

            // assert
            Assert.IsTrue(File.Exists(_filePath));

            List<string[]> lines = ReadSpreadsheetLines(_filePath);
            Assert.AreEqual("Valid,valid", String.Join(delimiter, lines[0]));
            Assert.AreEqual("DbNull,", String.Join(delimiter, lines[1]));
            Assert.AreEqual("null,", String.Join(delimiter, lines[2]));

            _excelRangeFormatter.Received(data.Length * data[0].Length).FormatCell(Arg.Any<ExcelRange>(), Arg.Any<object>(), Arg.Any<Type>());

        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WriteLine_WithoutInitialise_ThrowsException()
        {
            IDataReader reader = Substitute.For<IDataReader>();

            _reportWriter.WriteLine(reader, new ColumnMetaData[] { }, String.Empty);

            reader.Received(0).GetValue(Arg.Any<int>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void WriteHeader_WithoutInitialise_ThrowsException()
        {
            _reportWriter.WriteHeader(new string[] { }, String.Empty);
        }

        private List<string[]> ReadSpreadsheetLines(string filePath)
        {
            List<string[]> lines = new List<string[]>();
            using (ExcelPackage package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet sheet = package.Workbook.Worksheets.First();
                var rows = sheet.Dimension.End.Row;
                var cols = sheet.Dimension.End.Column;
                for (int r = 0; r < rows; r++)
                {
                    List<string> values = new List<string>();
                    for (int c = 0; c < cols; c++)
                    {
                        string val = (sheet.Cells[r + 1, c + 1].Value ?? "").ToString();
                        values.Add(val.ToString());
                    }
                    lines.Add(values.ToArray());
                }
            }
            return lines;
        }
    }
}
