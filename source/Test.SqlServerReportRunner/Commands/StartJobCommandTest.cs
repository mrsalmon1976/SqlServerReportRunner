using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Commands;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using SqlServerReportRunner.Reporting;

namespace Test.SqlServerReportRunner.Commands
{
    [TestFixture]
    public class StartJobCommandTest
    {
        private IStartJobCommand _startJobCommand;
        private IDbConnectionFactory _dbConnectionFactory;
        private IConcurrencyCoordinator _concurrencyCoordinator;

        [SetUp]
        public void StartJobCommandTest_SetUp()
        {
            _concurrencyCoordinator = Substitute.For<IConcurrencyCoordinator>();
            _dbConnectionFactory = Substitute.For<IDbConnectionFactory>();
            _startJobCommand = new StartJobCommand(_concurrencyCoordinator, _dbConnectionFactory);
        }

        [Test]
        public void Execute_WithoutDbConnection_Validate()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            int jobId = new Random().Next(100, 100000);
            ReportJob reportJob = new ReportJob() { Id = jobId };
            IDbConnection dbConnection = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConnection);

            // execute
            _startJobCommand.Execute(connSetting, jobId);

            // assert
            _dbConnectionFactory.Received(1).CreateConnection(connString);
            _concurrencyCoordinator.Received(1).LockReportJob(connSetting.Name, jobId);
            dbConnection.Received(1).Execute(Arg.Any<string>(), Arg.Any<object>(), null, null, null);

        }
    }
}
