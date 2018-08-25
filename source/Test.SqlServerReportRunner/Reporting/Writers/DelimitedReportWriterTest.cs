using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Formatters;
using SqlServerReportRunner.Reporting.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting.Writers
{
    [TestFixture]
    public class DelimitedReportWriterTest
    {
        private IReportWriter _reportWriter;
        private ITextFormatter _textFormatter;
        private string _testRootFolder = String.Empty;
        private string _filePath = String.Empty;

        [SetUp]
        public void DelimitedReportWriterTest_SetUp()
        {
            // create the root folder
            _testRootFolder = Path.Combine(TestUtility.TestRootFolder, "DelimitedReportWriterTest");
            Directory.CreateDirectory(_testRootFolder);

            _textFormatter = Substitute.For<ITextFormatter>();

            _filePath = Path.Combine(_testRootFolder, Path.GetRandomFileName());
            _reportWriter = new DelimitedReportWriter(_textFormatter);

        }

        [TearDown]
        public void DelimitedReportWriterTest_TearDown()
        {
            Directory.Delete(_testRootFolder, true);
        }

        [TestCase(",")]
        [TestCase("|")]
        [TestCase("********")]
        public void DelimitedReportWriterTest_CheckFileContents(string delimiter)
        {
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
            reader.Read().Returns(true, true, true, false);
            reader.GetValue(0).Returns(data[0][0], data[1][0], data[2][0]);
            reader.GetValue(1).Returns(data[0][1], data[1][1], data[2][1]);
            reader.GetValue(2).Returns(data[0][2], data[1][2], data[2][2]);
            reader.GetFieldType(Arg.Any<int>()).Returns(typeof(String));

            // set up the text formatter to return the value supplied
            _textFormatter.FormatText(Arg.Any<object>(), Arg.Any<Type>()).Returns((c) => { return c.ArgAt<object>(0).ToString(); });

            // execute

            _reportWriter.Initialise(_filePath);
            _reportWriter.WriteHeader(headers.Select(x => x.Name), delimiter);
            foreach (object[] line in data)
            {
                _reportWriter.WriteLine(reader, headers, delimiter);
            }
            _reportWriter.Dispose();

            // assert
            Assert.IsTrue(File.Exists(_filePath));

            List<string> lines = File.ReadLines(_filePath).ToList();

            // check that the header is correct
            string expectedHeader = String.Join(delimiter, headers.Select(x => x.Name));
            Assert.AreEqual(expectedHeader, lines[0]);

            // make sure each of the lines is written correctly
            for (int i=0; i<data.Length; i++)
            {
                object[] line = data[i];
                string expectedLine = String.Join(delimiter, data[i]);
                string actualLine = lines[i + 1];
                Assert.AreEqual(expectedLine, actualLine);
            }

            // make sure all the data items are formatted
            foreach (object[] line in data)
            {
                foreach (object item in line)
                {
                    _textFormatter.Received(1).FormatText(item, typeof(String));
                }
            }

        }

        [Test]
        public void WriteLine_WithoutInitialise_ThrowsException()
        {
            IDataReader reader = Substitute.For<IDataReader>();

            TestDelegate del = () => _reportWriter.WriteLine(reader, new ColumnMetaData[] { }, String.Empty);

            reader.Received(0).GetValue(Arg.Any<int>());
            Assert.Throws(typeof(InvalidOperationException), del);
        }

        [Test]
        public void WriteHeader_WithoutInitialise_ThrowsException()
        {
            TestDelegate del = () => _reportWriter.WriteHeader(new string[] { }, String.Empty);
            Assert.Throws(typeof(InvalidOperationException), del);
        }

    }
}
