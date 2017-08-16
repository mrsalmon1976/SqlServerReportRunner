using SqlServerReportRunner.Reporting;
using SqlServerReportRunner.Models;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace SqlServerReportRunner
{
    public interface IReportProcessorService
    {
        void Start();
        void Stop();
    }

    public class ReportProcessorService : IReportProcessorService
    {
        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private Thread _thread;

        public ReportProcessorService()
        {
        }

        public void Start()
        {
            // code that runs when the Windows Service starts up
            _thread = new Thread(WorkerThreadFunc)
            {
                Name = "Sql Server Report Runner Service",
                IsBackground = true
            };
            _thread.Start();
            _logger.Info("Service started");
        }

        public void Stop()
        {
            // code that runs when the Windows Service stops
            _logger.Info("Service stopped");
        }

        private void WorkerThreadFunc()
        {
            var container = TinyIoC.TinyIoCContainer.Current;
            var appSettings = container.Resolve<IAppSettings>();
            int pollInterval = appSettings.PollInterval;
            while (!_shutdownEvent.WaitOne(0))
            {
                int executedCount = 0;
                try
                {
                    var engine = container.Resolve<IReportEngine>();
                    IEnumerable<ReportJob> jobs = engine.ExecuteReports();
                    executedCount = jobs.Count();
                    _logger.Info("{0} jobs started", executedCount);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, ex.Message);
                }

                // sleep for poll interval
                Thread.Sleep(pollInterval);
            }
        }

    }
}
