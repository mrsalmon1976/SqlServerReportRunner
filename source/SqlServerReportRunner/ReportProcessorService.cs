using Nancy.TinyIoc;
using SqlServerReportRunner.Reporting;
using SqlServerReportRunner.Models;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Nancy.Hosting.Self;

namespace SqlServerReportRunner
{
    public interface IReportProcessorService
    {
        void Start();
        void Stop();
    }

    public class ReportProcessorService : IReportProcessorService
    {
        private NancyHost _host;

        private readonly ManualResetEvent _shutdownEvent = new ManualResetEvent(false);
        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private Thread _thread;

        private IAppSettings _appSettings;
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IReportEngine _reportEngine;

        public ReportProcessorService(IAppSettings appSettings, IConcurrencyCoordinator concurrencyCoordinator, IReportEngine reportEngine)
        {
            _appSettings = appSettings;
            _concurrencyCoordinator = concurrencyCoordinator;
            _reportEngine = reportEngine;
        }

        public void Start()
        {
            _logger.Info("Service starting");
            // set up the background thread for report processing
            _thread = new Thread(WorkerThreadFunc)
            {
                Name = "Sql Server Report Runner Service",
                IsBackground = true
            };
            _thread.Start();

            // set up the nancy host
            var hostConfiguration = new HostConfiguration
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            string url = String.Format("http://localhost:{0}", _appSettings.ConsolePort);
            _host = new NancyHost(hostConfiguration, new Uri(url));
            _host.Start();

            _logger.Info("Service started");
        }

        public void Stop()
        {
            _logger.Info("Service shutting down");
            _host.Stop();
            _host.Dispose();

            // code that runs when the Windows Service stops
            _logger.Info("Service stopped");
        }

        private void WorkerThreadFunc()
        {
            int pollInterval = _appSettings.PollInterval;

            // on start up, make sure we clear out any old locks if they exist - this is 
            // just in case there were issues, or the server rebooted.  We may end up with 
            // Report queue items in an odd state but that will need to be handled manually
            _concurrencyCoordinator.ClearAllLocks();

            while (!_shutdownEvent.WaitOne(0))
            {
                int executedCount = 0;
                try
                {
                    IEnumerable<ReportJob> jobs = _reportEngine.ExecuteReports();
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
