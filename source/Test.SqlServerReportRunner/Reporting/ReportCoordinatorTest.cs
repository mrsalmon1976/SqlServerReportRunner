using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner;
using SqlServerReportRunner.Repositories;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting
{
    [TestFixture]
    public class ReportCoordinatorTest
    {
        private IAppSettings _appSettings;
        private IReportJobAgent _reportJobAgent;
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IReportJobRepository _reportJobRepository;

        private IReportCoordinator _reportCoordinator;

        [SetUp]
        public void ReportCoordinatorTest_SetUp()
        {
            _appSettings = Substitute.For<IAppSettings>();
            _reportJobAgent = Substitute.For<IReportJobAgent>();
            _concurrencyCoordinator = Substitute.For<IConcurrencyCoordinator>();
            _reportJobRepository = Substitute.For<IReportJobRepository>();

            _reportCoordinator = new ReportCoordinator(_appSettings, _reportJobAgent, _concurrencyCoordinator, _reportJobRepository);
        }

        [Test]
        public void RunReports_MaxConcurrentReportsExceeded_ExitsAndReturnsEmptyCollection()
        {
            // setup
            string connName = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, "connection");

            _concurrencyCoordinator.GetRunningReports(connName).Returns(new int[] { 1, 2, 3, 4, 5 });
            _appSettings.MaxConcurrentReports.Returns(5);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(0, result.Count());
            _concurrencyCoordinator.Received(1).GetRunningReports(connName);

            // no call should have been made for more reports
            _reportJobRepository.DidNotReceive().GetPendingReports(Arg.Any<string>(), Arg.Any<int>());
            _concurrencyCoordinator.Received(0).LockReportJob(Arg.Any<string>(), Arg.Any<int>());
        }

        [Test]
        public void RunReports_ReportsRunningLessThanLimit_NewReportsRun()
        {
            // setup
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _concurrencyCoordinator.GetRunningReports(connName).Returns(new int[] { 1, 2 });
            _appSettings.MaxConcurrentReports.Returns(5);

            List<ReportJob> returnValue = new List<ReportJob>()
            {
                new ReportJob(),
                new ReportJob(),
                new ReportJob(),
            };
            _reportJobRepository.GetPendingReports(connectionString, 3).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(3, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetPendingReports(connectionString, 3);

            _concurrencyCoordinator.Received(1).GetRunningReports(connName);
            _reportJobAgent.Received(3).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
        }

        [Test]
        public void RunReports_NoReportsRunning_NewReportsRun()
        {
            // setup
            int maxConcurrentReports = new Random().Next(3, 8);
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _concurrencyCoordinator.GetRunningReports(connName).Returns(new int[] { });
            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            List<ReportJob> returnValue = new List<ReportJob>();
            for (int i=0; i<maxConcurrentReports; i++)
            {
                returnValue.Add(new ReportJob());
            };
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(maxConcurrentReports, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetPendingReports(connectionString, maxConcurrentReports);

            _concurrencyCoordinator.Received(1).GetRunningReports(connName);
            _reportJobAgent.Received(maxConcurrentReports).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
        }

        [Test]
        public void RunReports_NoReportsRunningButLessReportsAvailableThanMax_CorrectNumberOfNewReportsRun()
        {
            // setup
            const int maxConcurrentReports = 5;
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _concurrencyCoordinator.GetRunningReports(connName).Returns(new int[] { });
            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            List<ReportJob> returnValue = new List<ReportJob>();
            returnValue.Add(new ReportJob());
            returnValue.Add(new ReportJob());
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(2, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetPendingReports(connectionString, maxConcurrentReports);

            _concurrencyCoordinator.Received(1).GetRunningReports(connName);
            _reportJobAgent.Received(2).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
        }

    }
}
