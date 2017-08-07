using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Commands;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting
{
    [TestFixture]
    public class ReportJobAgentTest
    {
        private IReportJobAgent _reportJobAgent;
        private IReportExecutorFactory _reportExecutorFactory;
        private IStartJobCommand _startJobCommand;
        private IFinaliseJobCommand _finaliseJobCommand;
        private IFailJobCommand _failJobCommand;

        [SetUp]
        public void ReportJobAgentTest_SetUp()
        {
            _reportExecutorFactory = Substitute.For<IReportExecutorFactory>();
            _startJobCommand = Substitute.For<IStartJobCommand>();
            _finaliseJobCommand = Substitute.For<IFinaliseJobCommand>();
            _failJobCommand = Substitute.For<IFailJobCommand>();

            _reportJobAgent = new ReportJobAgent(_reportExecutorFactory, _startJobCommand, _finaliseJobCommand, _failJobCommand);
        }

        [Test]
        public void ExecuteJob_OnSuccess_FinalisesJob()
        {
            // setup
            ConnectionSetting connSetting = new ConnectionSetting("MyConn", "MyConnString");
            ReportJob reportJob = new ReportJob();
            reportJob.Id = new Random().Next(1, 100);
            reportJob.CommandType = Guid.NewGuid().ToString();
            reportJob.OutputFormat = Guid.NewGuid().ToString();

            IReportExecutor reportExecutor = Substitute.For<IReportExecutor>();
            _reportExecutorFactory.GetReportExecutor(reportJob.CommandType).Returns(reportExecutor);

            // execute
            _reportJobAgent.ExecuteJob(connSetting, reportJob);

            // assert
            _reportExecutorFactory.Received(1).GetReportExecutor(reportJob.CommandType);
            _startJobCommand.Received(1).Execute(connSetting, reportJob.Id);
            reportExecutor.Received(1).ExecuteJob(connSetting, reportJob);
            _finaliseJobCommand.Received(1).Execute(connSetting, reportJob.Id);

            _failJobCommand.Received(0).Execute(Arg.Any<ConnectionSetting>(), Arg.Any<int>(), Arg.Any<Exception>());
        }

        [Test]
        public void ExecuteJob_OnSuccess_FailsJob()
        {
            // setup
            ConnectionSetting connSetting = new ConnectionSetting("MyConn", "MyConnString");
            ReportJob reportJob = new ReportJob();
            reportJob.Id = new Random().Next(1, 100);
            reportJob.CommandType = Guid.NewGuid().ToString();
            reportJob.OutputFormat = Guid.NewGuid().ToString();

            IReportExecutor reportExecutor = Substitute.For<IReportExecutor>();
            _reportExecutorFactory.GetReportExecutor(reportJob.CommandType).Returns(reportExecutor);
            reportExecutor.When(x => x.ExecuteJob(Arg.Any<ConnectionSetting>(), Arg.Any<ReportJob>())).Do((c) => { throw new Exception(); });

            // execute
            _reportJobAgent.ExecuteJob(connSetting, reportJob);

            // assert
            _reportExecutorFactory.Received(1).GetReportExecutor(reportJob.CommandType);
            _startJobCommand.Received(1).Execute(connSetting, reportJob.Id);
            _failJobCommand.Received(1).Execute(connSetting, reportJob.Id, Arg.Any<Exception>());
            _finaliseJobCommand.Received(0).Execute(Arg.Any<ConnectionSetting>(), Arg.Any<int>());

        }

    }
}
