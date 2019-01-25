using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.ViewModels.Dashboard
{
    public class StatisticsViewModel
    {
        public double AverageExecutionSeconds { get; set; }

        public double AverageGenerationSeconds { get; set; }

        public int TotalReportCount { get; set; }

        public IEnumerable<ReportCount> MostActiveUsers { get; set; }

        public IEnumerable<ReportCount> MostRunReports { get; set; }

        public IEnumerable<ReportCount> ReportCountByDay { get; set; }

        public int ErrorCount { get; set; }

    }
}
