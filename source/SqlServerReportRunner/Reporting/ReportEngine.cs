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
        private IReportCoordinator _reportCoordinator;

        public ReportEngine(IAppSettings appSettings, IReportCoordinator reportCoordinator)
        {
            _appSettings = appSettings;
            _reportCoordinator = reportCoordinator;
        }

        public IEnumerable<ReportJob> ExecuteReports()
        {
            List<ReportJob> executedJobs = new List<ReportJob>();

            IEnumerable<ConnectionSetting> connections = _appSettings.ConnectionSettings;
            foreach (ConnectionSetting conn in connections)
            {
                IEnumerable<ReportJob> jobs = _reportCoordinator.RunReports(conn);
                executedJobs.AddRange(jobs);
            }

            _logger.Info("{0} jobs executed across all connections", executedJobs.Count);
            return executedJobs;
        }
     
    }
}
