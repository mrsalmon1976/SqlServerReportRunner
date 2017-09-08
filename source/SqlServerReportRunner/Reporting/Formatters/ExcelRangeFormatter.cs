using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
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

            //CultureInfo cultureInfo = _appSettings.GlobalizationCulture;
            //string dateFormat = DateTime.Now.ToString(cultureInfo.DateTimeFormat.ShortDatePattern);
            if (dataType == typeof(DateTime))
            {
                range.Style.Numberformat.Format = _appSettings.ExcelDefaultDateTimeFormat;
            }
            range.Value = cellValue;
            return range;
        }

    }
}
