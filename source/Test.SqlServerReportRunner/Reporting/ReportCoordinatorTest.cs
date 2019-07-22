using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner;
using SqlServerReportRunner.BLL.Repositories;
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

        [TestCase(5)]
        [TestCase(6)]
        public void RunReports_MaxConcurrentReportsExceeded_ExitsAndReturnsEmptyCollection(int runningReportCount)
        {
            // setup
            string connName = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, "connection");

            _appSettings.MaxConcurrentReports.Returns(5);
            List<ReportJob> executingJobs = CreateReportJobList(runningReportCount);
            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(executingJobs);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(0, result.Count());
            _reportJobRepository.Received(1).GetProcessingReports(conn.ConnectionString);

            // no call should have been made for more reports
            _reportJobRepository.DidNotReceive().GetPendingReports(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<IEnumerable<string>>());
        }

        [Test]
        public void RunReports_ReportsRunningLessThanLimit_NewReportsRun()
        {
            // setup
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _appSettings.MaxConcurrentReports.Returns(5);
            List<ReportJob> executingJobs = CreateReportJobList(2);
            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(executingJobs);

            List<ReportJob> returnValue = CreateReportJobList(3);
            _reportJobRepository.GetPendingReports(connectionString, 3, Arg.Any<IEnumerable<string>>()).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(3, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetProcessingReports(conn.ConnectionString);
            _reportJobRepository.Received(1).GetPendingReports(connectionString, 3, Arg.Any<IEnumerable<string>>());
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

            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(new List<ReportJob>());
            List<ReportJob> returnValue = CreateReportJobList(maxConcurrentReports);
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>()).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(maxConcurrentReports, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>());
            _reportJobAgent.Received(maxConcurrentReports).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
        }

        [Test]
        public void RunReports_SingleExecutionGroupReportsRunning_CorrectlyPassedToRepository()
        {
            // setup
            int maxConcurrentReports = new Random().Next(3, 8);
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            string execGroup = Guid.NewGuid().ToString();
            List<ReportJob> processingReports = CreateReportJobList(5);
            processingReports[0].SingleExecutionGroup = execGroup;

            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(processingReports);
            List<ReportJob> returnValue = CreateReportJobList(maxConcurrentReports);
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>()).Returns(returnValue);

            _reportJobRepository.When(x => x.GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>())).Do(x =>
            {
                var r = x.ArgAt<IEnumerable<ReportJob>>(2);
                Assert.IsNotNull(r.FirstOrDefault(xr => xr.SingleExecutionGroup == execGroup));
            });

            // execute
            _reportCoordinator.RunReports(conn);
        }

        [Test]
        public void RunReports_ReportsToExecuteShareSingleExecutionGroup_SecondReportWithGroupNotRun()
        {
            // setup
            int maxConcurrentReports = new Random().Next(3, 8);
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            string reportName1 = Guid.NewGuid().ToString();
            string reportName2 = Guid.NewGuid().ToString();
            string reportName3 = Guid.NewGuid().ToString();

            // reports returned will have 3 reports, 2 of which share an execution group
            string execGroup = Guid.NewGuid().ToString();
            List<ReportJob> pendingReports = CreateReportJobList(3);
            pendingReports[0].SingleExecutionGroup = execGroup;
            pendingReports[0].ReportName = reportName1;
            pendingReports[1].SingleExecutionGroup = execGroup;
            pendingReports[1].ReportName = reportName2;
            pendingReports[2].ReportName = reportName3;

            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(Enumerable.Empty<ReportJob>());
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>()).Returns(pendingReports);

            // execute
            List<ReportJob> processedJobs = _reportCoordinator.RunReports(conn).ToList();
            Assert.AreEqual(2, processedJobs.Count);
            _reportJobAgent.Received(2).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
            Assert.IsNotNull(processedJobs.SingleOrDefault(x => x.ReportName == reportName1));
            Assert.IsNull(processedJobs.SingleOrDefault(x => x.ReportName == reportName2));
            Assert.IsNotNull(processedJobs.SingleOrDefault(x => x.ReportName == reportName3));
        }

        [Test]
        public void RunReports_NoReportsRunningButLessReportsAvailableThanMax_CorrectNumberOfNewReportsRun()
        {
            // setup
            const int maxConcurrentReports = 5;
            string connName = Guid.NewGuid().ToString();
            string connectionString = Guid.NewGuid().ToString();
            ConnectionSetting conn = new ConnectionSetting(connName, connectionString);

            _reportJobRepository.GetProcessingReports(conn.ConnectionString).Returns(new List<ReportJob>());
            _appSettings.MaxConcurrentReports.Returns(maxConcurrentReports);

            List<ReportJob> returnValue = CreateReportJobList(2);
            _reportJobRepository.GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>()).Returns(returnValue);

            // execute
            var result = _reportCoordinator.RunReports(conn);

            // assert
            Assert.AreEqual(2, result.Count());

            // no call should have been made for more reports
            _reportJobRepository.Received(1).GetPendingReports(connectionString, maxConcurrentReports, Arg.Any<IEnumerable<string>>());

            _reportJobAgent.Received(2).ExecuteJobAsync(conn, Arg.Any<ReportJob>());
        }

        private List<ReportJob> CreateReportJobList(int count)
        {
            List<ReportJob> executingJobs = new List<ReportJob>();
            for (int i = 0; i < count; i++)
            {
                executingJobs.Add(new ReportJob());
            }
            return executingJobs;
        }

    }
}
