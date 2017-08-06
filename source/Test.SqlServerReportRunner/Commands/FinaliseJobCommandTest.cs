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
    public class FinaliseJobCommandTest
    {
        private IFinaliseJobCommand _finaliseJobCommand;
        private IDbConnectionFactory _dbConnectionFactory;
        private IConcurrencyCoordinator _concurrencyCoordinator;

        [SetUp]
        public void FailJobCommandTest_SetUp()
        {
            _concurrencyCoordinator = Substitute.For<IConcurrencyCoordinator>();
            _dbConnectionFactory = Substitute.For<IDbConnectionFactory>();
            _finaliseJobCommand = new FinaliseJobCommand(_concurrencyCoordinator, _dbConnectionFactory);
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
            _finaliseJobCommand.Execute(connSetting, jobId);

            // assert
            _dbConnectionFactory.Received(1).CreateConnection(connString);
            _concurrencyCoordinator.Received(1).UnlockReportJob(connSetting.Name, jobId);
            dbConnection.Received(1).Execute(Arg.Any<string>(), Arg.Any<object>(), null, null, null);

        }
    }
}
