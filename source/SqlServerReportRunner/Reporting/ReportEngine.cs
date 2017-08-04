using SqlServerReportRunner.Models;
using NLog;
using System.Collections.Generic;

namespace SqlServerReportRunner.Reporting
{
    public interface IReportEngine
    {
        IEnumerable<ReportJob> ExecuteReports();
    }

    public class ReportEngine : IReportEngine
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IAppSettings _appSettings;
        private IReportCoordinator _reportRunCoordinator;

        public ReportEngine(IAppSettings appSettings, IReportCoordinator reportRunCoordinator)
        {
            _appSettings = appSettings;
            _reportRunCoordinator = reportRunCoordinator;
        }

        public IEnumerable<ReportJob> ExecuteReports()
        {
            List<ReportJob> executedJobs = new List<ReportJob>();

            IEnumerable<ConnectionSetting> connections = _appSettings.ConnectionSettings;
            foreach (ConnectionSetting conn in connections)
            {
                IEnumerable<ReportJob> jobs = _reportRunCoordinator.RunReports(conn);
                executedJobs.AddRange(jobs);
            }

            _logger.Info("{0} jobs executed across all connections", executedJobs.Count);
            return executedJobs;
        }
     
    }
}
