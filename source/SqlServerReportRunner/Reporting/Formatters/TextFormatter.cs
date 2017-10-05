using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Formatters
{
    public interface ITextFormatter
    {
        string FormatText(object value, Type dataType);
    }

    public class TextFormatter : ITextFormatter
    {
        private ILogger _logger = LogManager.GetCurrentClassLogger();
        private CultureInfo _dateTimeCultureInfo;
        private CultureInfo _numericCultureInfo;

        public TextFormatter(CultureInfo dateTimeCultureInfo, CultureInfo numericCultureInfo)
        {
            this._dateTimeCultureInfo = dateTimeCultureInfo;
            this._numericCultureInfo = numericCultureInfo;
        }

        public string FormatText(object value, Type dataType)
        {
            // for null values from the database - just exit
            if (value is DBNull || value == null)
            {
                return String.Empty;
            }

            // date/time and numbers needs to be formatted per globalization values
            try
            {
                if (dataType == typeof(DateTime))
                {
                    return Convert.ToDateTime(value).ToString(_dateTimeCultureInfo);
                }
                else if (dataType == typeof(Decimal))
                {
                    return Convert.ToDecimal(value).ToString(_numericCultureInfo);
                }
                else if (dataType == typeof(Single))
                {
                    return Convert.ToSingle(value).ToString(_numericCultureInfo);
                }
                else if (dataType == typeof(Double))
                {
                    return Convert.ToDouble(value).ToString(_numericCultureInfo);
                }
            }
            catch (Exception ex)
            {
                // we just swallow this exception
                _logger.Error(ex, ex.Message);
            }

            // for anything else, remove spaces!
            return value.ToString().Replace("\n", "").Replace("\r", "");
        }
    }
}
