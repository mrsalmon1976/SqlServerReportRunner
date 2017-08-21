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
    public class ReportWriterFactoryTest
    {
        private IReportWriterFactory _reportWriterFactory;

        [SetUp]
        public void ReportWriterFactoryTest_SetUp()
        {
            _reportWriterFactory = new ReportWriterFactory();

        }

        [TearDown]
        public void ReportWriterFactoryTest_TearDown()
        {
        }

        [TestCase("csv", typeof(CsvReportWriter))]
        [TestCase("CSV", typeof(CsvReportWriter))]
        [TestCase("Csv", typeof(CsvReportWriter))]
        [TestCase("delimited", typeof(DelimitedReportWriter))]
        [TestCase("Delimited", typeof(DelimitedReportWriter))]
        [TestCase("DELIMITED", typeof(DelimitedReportWriter))]
        [TestCase("excel", typeof(ExcelReportWriter))]
        [TestCase("Excel", typeof(ExcelReportWriter))]
        [TestCase("EXCEL", typeof(ExcelReportWriter))]
        public void GetReportWriter_SupportedReportFormat_ReturnsCorrectType(string reportFormat, Type expectedType)
        {
            string path = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());

            IReportWriter writer = _reportWriterFactory.GetReportWriter(path, reportFormat);

            Assert.IsInstanceOf(expectedType, writer);
            Assert.AreEqual(path, writer.FilePath);
        }

        [Test]
        [ExpectedException(ExpectedException =typeof(NotImplementedException))]
        public void GetReportWriter_UnsupportedReportFormat_ThrowsException()
        {
            string path = Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName());
            _reportWriterFactory.GetReportWriter(path, "nomnomnom");

        }


    }
}
