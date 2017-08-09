using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting.Writers
{
    [TestFixture]
    public class ExcelReportWriterTest
    {
        private IReportWriter _reportWriter;
        private string _testRootFolder = String.Empty;
        private string _filePath = String.Empty;

        [SetUp]
        public void ExcelReportWriterTest_SetUp()
        {
            // create the root folder
            _testRootFolder = Path.Combine(Environment.CurrentDirectory, "ExcelReportWriterTest");
            Directory.CreateDirectory(_testRootFolder);

            _filePath = Path.Combine(_testRootFolder, Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + ".xlsx");
            _reportWriter = new ExcelReportWriter(_filePath);

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
        }

        private List<string[]> ReadSpreadsheetLines(string filePath)
        {
            List<string[]> lines = new List<string[]>();
            using (XLWorkbook workbook = new XLWorkbook(filePath))
            {
                IXLWorksheet sheet = workbook.Worksheets.First();
                var rows = sheet.RowsUsed().Count();
                var cols = sheet.ColumnsUsed().Count();
                for (int r = 0; r < rows; r++)
                {
                    List<string> values = new List<string>();
                    for (int c = 0; c < cols; c++)
                    {
                        values.Add(sheet.Cell(r + 1, c + 1).Value.ToString());
                    }
                    lines.Add(values.ToArray());
                }
            }
            return lines;
        }
    }
}
