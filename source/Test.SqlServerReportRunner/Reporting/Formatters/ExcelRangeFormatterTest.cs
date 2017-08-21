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
        private IAppSettings _appSettings;



        [SetUp]
        public void ExcelRangeFormatterTest_SetUp()
        {
            _appSettings = Substitute.For<IAppSettings>();

            _excelRangeFormatter = new ExcelRangeFormatter(_appSettings);

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
        public void FormatCell_DataTypeIsDateTime_ValueAndStyleSet()
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            DateTime cellValue = DateTime.Now;
            string dateFormat = "yyyy-MM-dd";
            _appSettings.DefaultDateTimeFormat.Returns(dateFormat);

            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, typeof(DateTime));

            // assert
            Assert.AreEqual(cellValue, result.Value);
            Assert.AreEqual(dateFormat, result.Style.Numberformat.Format);

        }

        [Test]
        public void FormatCell_DataTypeIsString_ValueIsSet()
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

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsSingle_ValueIsFormatted(string numericFormat)
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            const float cellValue = 123.456789F;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, typeof(Single));

            // assert

            string expectedResult = cellValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result.Text);
        }

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsDouble_ValueIsFormatted(string numericFormat)
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            const double cellValue = 123.456789D;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, typeof(Double));

            // assert

            string expectedResult = cellValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result.Text);
        }

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsDecimal_ValueIsFormatted(string numericFormat)
        {
            // setup
            ExcelRange range = _workSheet.Cells[1, 1];
            const decimal cellValue = 123.456789M;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            ExcelRange result = _excelRangeFormatter.FormatCell(range, cellValue, typeof(Decimal));

            // assert

            string expectedResult = cellValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result.Text);
        }

    }
}
