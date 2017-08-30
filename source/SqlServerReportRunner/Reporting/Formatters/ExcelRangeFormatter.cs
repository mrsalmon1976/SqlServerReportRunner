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

        public ExcelRangeFormatter()
        {
        }

        public ExcelRange FormatCell(ExcelRange range, object cellValue, Type dataType)
        {
            // for null values from the database - just exit
            if (cellValue is DBNull || cellValue == null)
            {
                return range;
            }

            range.Value = cellValue;
            return range;
        }

    }
}
