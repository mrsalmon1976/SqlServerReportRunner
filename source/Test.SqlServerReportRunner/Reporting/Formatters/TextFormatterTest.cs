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


        [SetUp]
        public void TextFormatterTest_SetUp()
        {
            _textFormatter = new TextFormatter(null, null);
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

        [TestCase("en-ZA")]
        [TestCase("en-GB")]
        [TestCase("en-US")]
        [TestCase("ta-IN")]
        public void FormatCell_DataTypeIsDateTimeGlobalizationConfigured_ReturnsFormattedDateTime(string cultureName)
        {
            // setup 
            DateTime cellValue = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            _textFormatter = new TextFormatter(cultureInfo, null);

            // execute
            string result = _textFormatter.FormatText(cellValue, typeof(DateTime));

            // assert
            string expectedResult = cellValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

        [Test()]
        public void FormatCell_DataTypeIsDateTimeGlobalizationNotConfigured_ReturnsFormattedDateTime()
        {
            // setup 
            DateTime cellValue = DateTime.Now;
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            _textFormatter = new TextFormatter(cultureInfo, null);

            // execute
            string result = _textFormatter.FormatText(cellValue, typeof(DateTime));

            // assert
            string expectedResult = cellValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
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

        [TestCase("en-ZA")]
        [TestCase("en-GB")]
        [TestCase("en-US")]
        [TestCase("ta-IN")]
        public void FormatCell_DataTypeIsDoubleAndGlobalizationConfigured_ReturnsFormattedNumber(string cultureName)
        {
            // setup
            const double itemValue = 123.456789D;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            _textFormatter = new TextFormatter(null, cultureInfo);

            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Double));

            // assert

            string expectedResult = itemValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("en-ZA")]
        [TestCase("en-GB")]
        [TestCase("en-US")]
        [TestCase("ta-IN")]
        public void FormatCell_DataTypeIsSingleAndGlobalizationConfigured_ReturnsFormattedNumber(string cultureName)
        {
            // setup
            const float itemValue = 123.456789F;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            _textFormatter = new TextFormatter(null, cultureInfo);

            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Single));

            // assert

            string expectedResult = itemValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

        [TestCase("en-ZA")]
        [TestCase("en-GB")]
        [TestCase("en-US")]
        [TestCase("ta-IN")]
        public void FormatCell_DataTypeIsDecimalAndGlobalizationConfigured_ReturnsFormattedNumber(string cultureName)
        {
            // setup
            const decimal itemValue = 123.456789M;
            CultureInfo cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            _textFormatter = new TextFormatter(null, cultureInfo);

            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Decimal));

            // assert

            string expectedResult = itemValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void FormatCell_DataTypeIsNumericAndGlobalizationNotConfigured_ReturnsFormattedNumber()
        {
            // setup
            const decimal itemValue = 123.456789M;
            CultureInfo cultureInfo = CultureInfo.InvariantCulture;
            _textFormatter = new TextFormatter(null, cultureInfo);

            // execute
            string result = _textFormatter.FormatText(itemValue, typeof(Decimal));

            // assert

            string expectedResult = itemValue.ToString(cultureInfo);
            Assert.AreEqual(expectedResult, result);
        }

    }
}
