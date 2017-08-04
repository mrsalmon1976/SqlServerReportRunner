using NLog;
using SqlServerReportRunner.DAL.Repositories;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting
{
    public interface IReportJobAgent
    {
        void ExecuteJobAsync(string connectionString, ReportJob job);
        void ExecuteJob(string connectionString, ReportJob job);
    }

    public class ReportJobAgent : IReportJobAgent
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IReportExecutorFactory _reportExecutorFactory;
        private IReportJobRepository _reportJobRepository;

        public ReportJobAgent(IReportExecutorFactory reportExecutorFactory, IReportJobRepository reportJobRepository)
        {
            _reportExecutorFactory = reportExecutorFactory;
            _reportJobRepository = reportJobRepository;
        }

        public void ExecuteJobAsync(string connectionString, ReportJob job)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += new DoWorkEventHandler(ExecuteJobAsyncInternal);
            worker.RunWorkerAsync(new JobArgs(connectionString, job));
            _logger.Info("Created background thread for execution of report {0} ({1})", job.ReportName, job.Id);

        }

        private void ExecuteJobAsyncInternal(object sender, DoWorkEventArgs e)
        {
            JobArgs args = (JobArgs)e.Argument;
            ExecuteJob(args.ConnectionString, args.Job);
        }

        public void ExecuteJob(string connectionString, ReportJob job)
        {
            try
            {
                // get the executor relevant to the command type
                IReportExecutor reportExecutor = _reportExecutorFactory.GetReportExecutor(job.CommandType);

                // mark the job as processing 
                _reportJobRepository.MarkJobAsProcessing(connectionString, job.Id);

                // extract the data into a data table
                _logger.Info("Processing job for report {0} ({1})", job.ReportName, job.Id);
                reportExecutor.ExecuteJob(connectionString, job);

                // mark the job as processed 
                _reportJobRepository.MarkJobAsProcessed(connectionString, job.Id);

                _logger.Info("Completed job for report {0} ({1})", job.ReportName, job.Id);
            }
            catch (Exception ex)
            {
                // mark the job as errored
                _reportJobRepository.MarkJobAsError(connectionString, job.Id);
                _logger.Error(ex, ex.Message);
            }
        }

        private class JobArgs
        {

            public JobArgs(string connectionString, ReportJob job)
            {
                this.ConnectionString = connectionString;
                this.Job = job;
            }

            public string ConnectionString { get; set; }

            public ReportJob Job { get; set; }
        }
    }
}
