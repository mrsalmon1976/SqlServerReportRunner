using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Executors
{
    public interface IReportExecutor
    {
        ReportJobResult ExecuteJob(ConnectionSetting connection, ReportJob job);
    }
}
