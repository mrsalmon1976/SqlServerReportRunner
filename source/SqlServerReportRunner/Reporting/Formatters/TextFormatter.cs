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
        private IAppSettings _appSettings;

        public TextFormatter(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public string FormatText(object value, Type dataType)
        {
            // for null values from the database - just exit
            if (value is DBNull || value == null)
            {
                return String.Empty;
            }

            // date/time needs to be formatted per default values
            if (dataType == typeof(DateTime))
            {
                try
                {
                    return Convert.ToDateTime(value).ToString(_appSettings.DefaultDateTimeFormat);
                }
                catch (Exception)
                {
                    return value.ToString();
                }
            }
            else if (dataType == typeof(Decimal) || dataType == typeof(Single) || dataType == typeof(Double))
            {
                try
                {
                    return Convert.ToDouble(value).ToString(_appSettings.DefaultDecimalFormat, _appSettings.GlobalizationCulture);
                }
                catch (Exception)
                {
                    return value.ToString();
                }
            }

            // for anything else, remove spaces!
            return value.ToString().Replace("\n", "").Replace("\r", "");
        }
    }
}
