using NSubstitute;
using NUnit.Framework;
using Constants = SqlServerReportRunner.Constants;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace Test.SqlServerReportRunner.Reporting.Executors
{
    [TestFixture]
    public class StoredProcedureReportExecutorTest
    {
        private IReportExecutor _reportExecutor;

        private IDbConnectionFactory _dbConnectionFactory;
        private IReportWriterFactory _reportWriterFactory;
        private IDbParameterUtility _dbParameterUtility;

        [SetUp]
        public void StoredProcedureReportExecutorTest_SetUp()
        {

            _dbConnectionFactory = Substitute.For<IDbConnectionFactory>();
            _reportWriterFactory = Substitute.For<IReportWriterFactory>();
            _dbParameterUtility = Substitute.For<IDbParameterUtility>();

            _reportExecutor = new StoredProcedureReportExecutor(_dbConnectionFactory, _reportWriterFactory, _dbParameterUtility);
        }

        [Test]
        public void ExecuteJob_OnExecute_CreatesConnectionUsingFactory()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            
            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            _dbConnectionFactory.Received(1).CreateConnection(connString, true);

        }

        [Test]
        public void ExecuteJob_OnExecute_CommandSettingsInitialisedCorrectly()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            dbConn.Received(1).CreateCommand();
            cmd.Received(1).CommandTimeout = 0;
            cmd.Received(1).CommandText = job.Command;
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase(Constants.CommandType.Sql)]
        public void ExecuteJob_JobCommandTypeNotStoredProcedure_CommandTypeNotSet(string jobCommandType)
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.CommandType = jobCommandType;

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            cmd.DidNotReceive().CommandType = Arg.Any<System.Data.CommandType>();
        }

        [Test]
        public void ExecuteJob_JobCommandTypeSetToStoredProcedure_CommandTypeSetCorrectly()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.CommandType = Constants.CommandType.StoredProcedure;

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            cmd.Received(1).CommandType = System.Data.CommandType.StoredProcedure;
        }

        [Test]
        public void ExecuteJob_NoDbParameters_CommandParametersNotSet()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.Parameters = Guid.NewGuid().ToString();
            
            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);
            
            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            _dbParameterUtility.ConvertXmlToDbParameters(job.Parameters).Returns(new SqlParameter[] { });

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            var temp = cmd.DidNotReceive().Parameters;
        }

        [Test]
        public void ExecuteJob_OneDbParameter_SingleCommandParameterSet()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.Parameters = Guid.NewGuid().ToString();

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            IDataParameterCollection parmColl = Substitute.For<IDataParameterCollection>();
            cmd.Parameters.Returns(parmColl);

            SqlParameter parm1 = new SqlParameter(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            _dbParameterUtility.ConvertXmlToDbParameters(job.Parameters).Returns(new SqlParameter[] { parm1 });

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            parmColl.Received(1).Add(parm1);
            parmColl.Received(1).Add(Arg.Any<IDbDataParameter>());
        }

        [Test]
        public void ExecuteJob_MultipleDbParameters_MultipleCommandParameterSet()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.Parameters = Guid.NewGuid().ToString();

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            IDataParameterCollection parmColl = Substitute.For<IDataParameterCollection>();
            cmd.Parameters.Returns(parmColl);

            SqlParameter parm1 = new SqlParameter(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            SqlParameter parm2 = new SqlParameter(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            SqlParameter parm3 = new SqlParameter(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
            _dbParameterUtility.ConvertXmlToDbParameters(job.Parameters).Returns(new SqlParameter[] { parm1, parm2, parm3 });

            // execute
            _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            parmColl.Received(1).Add(parm1);
            parmColl.Received(1).Add(parm2);
            parmColl.Received(1).Add(parm3);
            parmColl.Received(3).Add(Arg.Any<IDbDataParameter>());
        }

        [TestCase("")]
        [TestCase(null)]
        public void ExecuteJob_NoOutputFormatSpecified_CommandExecutedWithoutWriter(string jobOutputFormat)
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.OutputFormat = jobOutputFormat;

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            // execute
            ReportJobResult result = _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            cmd.Received(1).ExecuteNonQuery();
            cmd.DidNotReceive().ExecuteReader();

            Assert.AreEqual(0, result.RowCount);
            Assert.GreaterOrEqual(result.ExecutionTime.TotalMilliseconds, 0);
        }

        [Test]
        public void ExecuteJob_OutputFormatSpecified_CommandExecutedWithWriter()
        {
            // setup
            string connString = Guid.NewGuid().ToString();
            ConnectionSetting connSetting = new ConnectionSetting(Guid.NewGuid().ToString(), connString);
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.OutputFormat = Guid.NewGuid().ToString();
            job.OutputFileName = Guid.NewGuid().ToString();
            job.OutputFilePath = Environment.CurrentDirectory;
            job.Delimiter = Guid.NewGuid().ToString();

            IDbConnection dbConn = Substitute.For<IDbConnection>();
            _dbConnectionFactory.CreateConnection(connString, true).Returns(dbConn);

            IDbCommand cmd = Substitute.For<IDbCommand>();
            dbConn.CreateCommand().Returns(cmd);

            IReportWriter reportWriter = Substitute.For<IReportWriter>();
            _reportWriterFactory.GetReportWriter(job.OutputFormat).Returns(reportWriter);

            IDataReader reader = Substitute.For<IDataReader>();
            reader.GetSchemaTable().Returns(new DataTable());
            cmd.ExecuteReader().Returns(reader);

            // execute
            ReportJobResult result = _reportExecutor.ExecuteJob(connSetting, job);

            // assert
            cmd.DidNotReceive().ExecuteNonQuery();
            cmd.Received(1).ExecuteReader();

            reportWriter.Received(1).Initialise(Arg.Any<String>());
            reportWriter.Received(1).WriteHeader(Arg.Any<IEnumerable<String>>(), job.Delimiter);

        }
    }
}
