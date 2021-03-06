﻿using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.BLL.Commands;
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

namespace Test.SqlServerReportRunner.BLL.Commands
{
    [TestFixture]
    public class FailJobCommandTest
    {
        private IFailJobCommand _failJobCommand;
        private IDbConnectionFactory _dbConnectionFactory;
        private IConcurrencyCoordinator _concurrencyCoordinator;

        [SetUp]
        public void FailJobCommandTest_SetUp()
        {
            _concurrencyCoordinator = Substitute.For<IConcurrencyCoordinator>();
            _dbConnectionFactory = Substitute.For<IDbConnectionFactory>();
            _failJobCommand = new FailJobCommand(_concurrencyCoordinator, _dbConnectionFactory);
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
            _failJobCommand.Execute(connSetting, jobId, new Exception("test exception"));

            // assert
            _dbConnectionFactory.Received(1).CreateConnection(connString);
            _concurrencyCoordinator.Received(1).UnlockReportJob(connSetting.Name, jobId);
            dbConnection.Received(1).Execute(Arg.Any<string>(), Arg.Any<object>(), null, null, null);

        }
    }
}
