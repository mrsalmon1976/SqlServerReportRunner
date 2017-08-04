using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Models
{
    public class ReportJobResult
    {
        public int RowCount { get; set; }

        public TimeSpan ExecutionTime { get; set; }
    }
}
