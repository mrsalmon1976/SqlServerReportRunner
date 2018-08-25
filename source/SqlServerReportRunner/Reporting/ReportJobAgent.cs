using NLog;
using SqlServerReportRunner.BLL.Repositories;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlServerReportRunner.BLL.Commands;
using SqlServerReportRunner.Common;
using System.Data;

namespace SqlServerReportRunner.Reporting
{
    public interface IReportJobAgent
    {
        void ExecuteJobAsync(ConnectionSetting connection, ReportJob job);
        void ExecuteJob(ConnectionSetting connection, ReportJob job);
    }

    public class ReportJobAgent : IReportJobAgent
    {
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private IReportExecutorFactory _reportExecutorFactory;
        private IStartJobCommand _startJobCommand;
        private IFinaliseJobCommand _finaliseJobCommand;
        private IFailJobCommand _failJobCommand;

        public ReportJobAgent(IReportExecutorFactory reportExecutorFactory, IStartJobCommand startJobCommand, IFinaliseJobCommand finaliseJobCommand, IFailJobCommand failJobCommand)
        {
            _reportExecutorFactory = reportExecutorFactory;
            _startJobCommand = startJobCommand;
            _finaliseJobCommand = finaliseJobCommand;
            _failJobCommand = failJobCommand;
        }

        public void ExecuteJobAsync(ConnectionSetting connection, ReportJob job)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = false;
            worker.DoWork += new DoWorkEventHandler(ExecuteJobAsyncInternal);
            worker.RunWorkerAsync(new JobArgs(connection, job));
            _logger.Info("Created background thread for execution of report {0} ({1})", job.ReportName, job.Id);

        }

        private void ExecuteJobAsyncInternal(object sender, DoWorkEventArgs e)
        {
            JobArgs args = (JobArgs)e.Argument;
            ExecuteJob(args.Connection, args.Job);
        }

        public void ExecuteJob(ConnectionSetting connection, ReportJob job)
        {
            try
            {
                // get the executor relevant to the command type
                IReportExecutor reportExecutor = _reportExecutorFactory.GetReportExecutor(job.CommandType);

                // mark the job as processing 
                _logger.Info("Moving job {0} (Id: {1}) into processing state", job.ReportName, job.Id);
                _startJobCommand.Execute(connection, job.Id);

                // extract the data into a data table
                _logger.Info("Processing job for report {0} ({1})", job.ReportName, job.Id);
                reportExecutor.ExecuteJob(connection, job);

                // mark the job as processed 
                _logger.Info("Marking job {0} (Id: {1}) as complete", job.ReportName, job.Id);
                _finaliseJobCommand.Execute(connection, job.Id);

                _logger.Info("Completed job for report {0} ({1})", job.ReportName, job.Id);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, ex.Message);

                // mark the job as errored
                _logger.Info("Saving error information job for report {0} ({1})", job.ReportName, job.Id);
                _failJobCommand.Execute(connection, job.Id, ex);
            }
        }

        private class JobArgs
        {

            public JobArgs(ConnectionSetting connection, ReportJob job)
            {
                this.Connection = connection;
                this.Job = job;
            }

            public ConnectionSetting Connection { get; set; }

            public ReportJob Job { get; set; }
        }
    }
}
