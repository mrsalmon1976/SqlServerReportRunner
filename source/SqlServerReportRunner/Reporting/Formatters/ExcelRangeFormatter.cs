using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Formatters
{
    public interface IExcelRangeFormatter
    {
        ExcelRange FormatCell(ExcelRange range, object cellValue, Type dataType);

    }

    public class ExcelRangeFormatter : IExcelRangeFormatter
    {

        private IAppSettings _appSettings;

        public ExcelRangeFormatter(IAppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public ExcelRange FormatCell(ExcelRange range, object cellValue, Type dataType)
        {
            // for null values from the database - just exit
            if (cellValue is DBNull || cellValue == null)
            {
                return range;
            }

            // if data type is null, we set the type and exit
            if (dataType == null)
            {
                range.Value = cellValue;
                return range;
            }

            // for date time, we need to apply a default format
            if (dataType == typeof(DateTime))
            {
                if (!String.IsNullOrWhiteSpace(_appSettings.DefaultDateTimeFormat))
                {
                    range.Style.Numberformat.Format = _appSettings.DefaultDateTimeFormat;
                }
                try
                {
                    range.Value = Convert.ToDateTime(cellValue);
                }
                catch (Exception)
                {
                    range.Value = cellValue;
                }
                return range;
            }
            else if (dataType == typeof(Decimal) || dataType == typeof(Single) || dataType == typeof(Double))
            {
                if (!String.IsNullOrWhiteSpace(_appSettings.DefaultDecimalFormat))
                {
                    range.Style.Numberformat.Format = _appSettings.DefaultDecimalFormat;
                }
                range.Value = cellValue;
                return range;
            }


            // none of the above has been met - just set the value
            range.Value = cellValue;
            return range;
        }

    }
}
