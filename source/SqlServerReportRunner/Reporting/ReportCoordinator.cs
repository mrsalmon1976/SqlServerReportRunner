using NLog;
using SqlServerReportRunner.Repositories;
using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting
{
    public interface IReportCoordinator
    {
        IEnumerable<ReportJob> RunReports(ConnectionSetting connection);
    }

    public class ReportCoordinator : IReportCoordinator
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IAppSettings _appSettings;
        private IReportJobAgent _jobAgent;
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IReportJobRepository _reportJobRepository;

        public ReportCoordinator(IAppSettings appSettings, IReportJobAgent jobAgent, IConcurrencyCoordinator concurrencyCoordinator, IReportJobRepository reportJobRepository)
        {
            _appSettings = appSettings;
            _jobAgent = jobAgent;
            _concurrencyCoordinator = concurrencyCoordinator;
            _reportJobRepository = reportJobRepository;

        }

        public IEnumerable<ReportJob> RunReports(ConnectionSetting connection)
        {
            List<ReportJob> executedJobs = new List<ReportJob>();

            _logger.Debug("Retrieving unprocessed reports for connection {0}", connection.Name);

            // get the number of reports that are currently procesing for this connection
            int executingReports = _concurrencyCoordinator.GetRunningReportCount(connection.Name);
            if (executingReports >= _appSettings.MaxConcurrentReports)
            {
                _logger.Info("Maximum number of concurrent reports ({0}) are already being run for connection '{1}' exiting", _appSettings.MaxConcurrentReports, connection.Name);
                return Enumerable.Empty<ReportJob>();
            }

            int reportsToRun = _appSettings.MaxConcurrentReports - executingReports;

            _logger.Info("{0} reports executing for connection '{1}', checking queue for additional reports", executingReports, connection.Name);
            IEnumerable<ReportJob> jobs = _reportJobRepository.GetPendingReports(connection.ConnectionString, reportsToRun);
            foreach (ReportJob job in jobs)
            {
                _concurrencyCoordinator.LockReportJob(connection.Name, job.Id);
                _jobAgent.ExecuteJobAsync(connection.ConnectionString, job);
                executedJobs.Add(job);
            }

            return executedJobs;

        }
    }
}
