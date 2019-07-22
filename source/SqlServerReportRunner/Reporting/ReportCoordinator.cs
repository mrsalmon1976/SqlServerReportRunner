using NLog;
using SqlServerReportRunner.BLL.Repositories;
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
            List<ReportJob> executingReports = _reportJobRepository.GetProcessingReports(connection.ConnectionString).ToList();
            if (executingReports.Count >= _appSettings.MaxConcurrentReports)
            {
                _logger.Info("Maximum number of concurrent reports ({0}) are already being run for connection '{1}' exiting", _appSettings.MaxConcurrentReports, connection.Name);
                return Enumerable.Empty<ReportJob>();
            }

            // extract the list of SingleExecutionGroups that are running
            List<string> singleExecutionGroups = executingReports.Where(x => !String.IsNullOrWhiteSpace(x.SingleExecutionGroup)).Select(x => x.SingleExecutionGroup).Distinct().ToList();
            _logger.Info("SingleExecutionGroup list to avoid [{0}]", String.Join(",", singleExecutionGroups));

            int reportsToRun = _appSettings.MaxConcurrentReports - executingReports.Count;

            _logger.Info("{0} reports executing for connection '{1}', checking queue for additional reports", executingReports.Count, connection.Name);
            IEnumerable<ReportJob> jobs = _reportJobRepository.GetPendingReports(connection.ConnectionString, reportsToRun, singleExecutionGroups);
            foreach (ReportJob job in jobs)
            {

                // if the job has a SingleExecutionGroup, we need to add it to the list so we are certain we don't run more 
                // than one of these - we also need to check it hasn't already been added by a previous job retrieved in 
                // the same call
                if (!String.IsNullOrWhiteSpace(job.SingleExecutionGroup))
                {
                    if (singleExecutionGroups.Contains(job.SingleExecutionGroup))
                    {
                        _logger.Info("Not running job '{0}' as a job with the same SingleExecutionGroup ('{1}') is already running", job.Id, job.SingleExecutionGroup);
                        continue;
                    }
                    else
                    {
                        singleExecutionGroups.Add(job.SingleExecutionGroup);
                        _logger.Info("Added SingleExecutionGroup '{0}' to list of groups to skip", job.SingleExecutionGroup);
                    }
                }

                _jobAgent.ExecuteJobAsync(connection, job);
                executedJobs.Add(job);
            }

            return executedJobs;

        }
    }
}
