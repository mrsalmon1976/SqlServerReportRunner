using SqlServerReportRunner.Common;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Executors
{
    public interface IReportWriterFactory
    {
        IReportWriter GetReportWriter(string filePath, string reportFormat);
    }
    public class ReportWriterFactory : IReportWriterFactory
    {

        public ReportWriterFactory()
        {
        }

        public IReportWriter GetReportWriter(string filePath, string reportFormat)
        {
            switch (reportFormat.ToLower())
            {
                case ReportFormat.Csv:
                    return new CsvReportWriter(filePath);
                case ReportFormat.Delimited:
                    return new DelimitedReportWriter(filePath);
                default:
                    break;
            }

            throw new NotImplementedException(String.Format("No report writer implemented for report format '{0}'", reportFormat));
        }

    }
}
