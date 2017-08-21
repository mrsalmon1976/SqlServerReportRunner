using NSubstitute;
using NUnit.Framework;
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
    public class TextFormatterTest
    {
        private ITextFormatter _textFormatter;
        private IAppSettings _appSettings;


        [SetUp]
        public void TextFormatterTest_SetUp()
        {
            _appSettings = Substitute.For<IAppSettings>();

            _textFormatter = new TextFormatter(_appSettings);
        }

        [Test]
        public void FormatText_DataIsDbNull_ReturnsEmptyString()
        {
            // setup 

            // execute
            string result = _textFormatter.FormatText(DBNull.Value, null);

            // assert
            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void FormatText_DataIsNull_ReturnsEmptyString()
        {
            // setup 

            // execute
            string result = _textFormatter.FormatText(null, null);

            // assert
            Assert.AreEqual(String.Empty, result);
        }

        [Test]
        public void FormatCell_DataTypeIsDateTime_ReturnsFormattedDateTime()
        {
            // setup 
            DateTime cellValue = DateTime.Now;
            const string dateFormat = "yyyy-MM-dd HH:mm:ss";
            _appSettings.DefaultDateTimeFormat.Returns(dateFormat);

            // execute
            string result = _textFormatter.FormatText(cellValue, typeof(DateTime));

            // assert
            Assert.AreEqual(cellValue.ToString(dateFormat), result);

        }

        [Test]
        public void FormatCell_DataTypeIsString_ReturnsString()
        {
            // setup 
            string cellValue = Guid.NewGuid().ToString();

            // execute
            string result = _textFormatter.FormatText(cellValue, typeof(String));

            // assert
            Assert.AreEqual(cellValue, result);

        }

        [TestCase("test\n1")]
        [TestCase("test\n\r2")]
        [TestCase("test\r\n3")]
        [TestCase("test\r4")]
        public void FormatCell_ValueContainsLineBreaks_ReturnsStringWithLineBreaksRemoved(string cellValue)
        {
            // setup 

            // execute
            string result = _textFormatter.FormatText(cellValue, typeof(String));

            // assert
            Assert.AreEqual(cellValue.Replace("\r", "").Replace("\n", ""), result);
        }

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsSingle_ReturnsFormattedNumber(string numericFormat)
        {
            // setup
            const float itemValue = 123.456789F;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Single));

            // assert

            string expectedResult = itemValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsDouble_ReturnsFormattedNumber(string numericFormat)
        {
            // setup
            const double itemValue = 123.456789D;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Double));

            // assert

            string expectedResult = itemValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("0.0000")]
        [TestCase("0.00")]
        [TestCase("0")]
        public void FormatCell_DataTypeIsDecimal_ReturnsFormattedNumber(string numericFormat)
        {
            // setup
            const decimal itemValue = 123.456789M;
            CultureInfo culture = CultureInfo.CurrentCulture;

            _appSettings.DefaultDecimalFormat.Returns(numericFormat);
            _appSettings.GlobalizationCulture.Returns(culture);
            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Decimal));

            // assert
            string expectedResult = itemValue.ToString(numericFormat, culture);
            Assert.AreEqual(expectedResult, result);
        }

    }
}
