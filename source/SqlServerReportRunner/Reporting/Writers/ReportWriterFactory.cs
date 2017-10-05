using SqlServerReportRunner.Common;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Writers;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Writers
{
    public interface IReportWriterFactory
    {
        IReportWriter GetReportWriter(string reportFormat);
    }
    public class ReportWriterFactory : IReportWriterFactory
    {

        public IReportWriter GetReportWriter(string reportFormat)
        {
            IAppSettings appSettings = new AppSettings();

            switch (reportFormat.ToLower())
            {
                case ReportFormat.Csv:
                    return new CsvReportWriter(new TextFormatter(appSettings.GlobalizationCultureDateTime, appSettings.GlobalizationCultureNumeric));
                case ReportFormat.Delimited:
                    return new DelimitedReportWriter(new TextFormatter(appSettings.GlobalizationCultureDateTime, appSettings.GlobalizationCultureNumeric));
                case ReportFormat.Excel:
                    return new ExcelReportWriter(new ExcelRangeFormatter(appSettings.ExcelDefaultDateTimeFormat));
                default:
                    break;
            }

            throw new NotImplementedException(String.Format("No report writer implemented for report format '{0}'", reportFormat));
        }

    }
}
