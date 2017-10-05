using NSubstitute;
using NUnit.Framework;
using OfficeOpenXml;
using SqlServerReportRunner;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting.Formatters
{
    [TestFixture]
    public class ExcelRangeFormatterTest
    {
        private ExcelPackage _excelPackage;
        private ExcelWorksheet _workSheet;
        private IExcelRangeFormatter _excelRangeFormatter;

        [SetUp]
        public void ExcelRangeFormatterTest_SetUp()
        {
            _excelRangeFormatter = new ExcelRangeFormatter(null);

            _excelPackage = new ExcelPackage();
            _workSheet = _excelPackage.Workbook.Worksheets.Add("Data");
        }

        [TearDown]
        public void ExcelRangeFormatterTest_TearDown()
        {
            _workSheet.Dispose();
            _excelPackage.Dispose();
        }

        [Test]
        public void FormatCell_DataIsDbNull_RangeReturnedUntouched()
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            range.Value = "test";

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, DBNull.Value, null);

            // assert
            Assert.AreEqual(range.Value, result.Value);
            Assert.AreEqual("General", result.Style.Numberformat.Format);
        }

        [Test]
        public void FormatCell_DataIsNull_RangeReturnedUntouched()
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            range.Value = "test";

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, null, null);

            // assert
            Assert.AreEqual(range.Value, result.Value);
            Assert.AreEqual("General", result.Style.Numberformat.Format);
        }

        [Test]
        public void FormatCell_DataTypeIsNull_OnlyValueIsSet()
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            string cellValue = Guid.NewGuid().ToString();

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, null);

            // assert
            Assert.AreEqual(cellValue, result.Value);
            Assert.AreEqual("General", result.Style.Numberformat.Format);

        }

        [Test]
        public void FormatCell_DataTypeIsStringAndDefaultConfigured_ValueIsSet()
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            string cellValue = Guid.NewGuid().ToString();

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, typeof(String));

            // assert
            Assert.AreEqual(cellValue, result.Text);
            Assert.AreEqual("General", result.Style.Numberformat.Format);
        }

        [Test()]
        public void FormatCell_DataTypeIsDateTime_ExcelDefaultFormatIsSetOnRange()
        {
            // setup 
            const string dateFormat = "yyyy-MM-dd HH:mm:ss";
            _excelRangeFormatter = new ExcelRangeFormatter(dateFormat);

            ExcelRange range = _workSheet.Cells[1, 1];

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, DateTime.Now, typeof(DateTime));

            // assert
            Assert.AreEqual(dateFormat, result.Style.Numberformat.Format);
        }

    }
}
