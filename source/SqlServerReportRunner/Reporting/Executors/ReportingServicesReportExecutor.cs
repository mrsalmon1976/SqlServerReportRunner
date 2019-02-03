using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.IO;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Reporting.Writers;
using SqlServerReportRunner.BLL.Net;
using NLog;
using System.Net;

namespace SqlServerReportRunner.Reporting.Executors
{
    public class ReportingServicesReportExecutor : IReportExecutor
    {
        private IWebClientWrapper _webClientWrapper;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public ReportingServicesReportExecutor(IWebClientWrapper webClientWrapper)
        {
            _webClientWrapper = webClientWrapper;
        }

        public ReportJobResult ExecuteJob(ConnectionSetting connection, ReportJob job)
        {
            DateTime start = DateTime.UtcNow;

            string url = job.Command;
            string outputhFilePath = Path.Combine(job.OutputFilePath, job.OutputFileName);

            _logger.Info("Sending web request to {0} as user {1}", url, CredentialCache.DefaultNetworkCredentials.UserName);

            _webClientWrapper.UseDefaultCredentials = true;
            _webClientWrapper.DownloadFile(url, outputhFilePath);

            _logger.Info("Reporting services file saved to {0}", outputhFilePath);

            ReportJobResult result = new ReportJobResult();
            result.RowCount = -1;
            result.ExecutionTime = DateTime.UtcNow.Subtract(start);
            return result;
        }

    }
}
