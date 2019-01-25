using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Reporting;
using SqlServerReportRunner.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.BLL.Repositories
{
    [TestFixture]
    [Category("RequiresDatabase")]
    public class ReportJobRepositoryTest
    {
        private IReportJobRepository _reportJobRepository;

        private IDbConnectionFactory _dbConnectionFactory;
        [SetUp]
        public void ReportJobRepositoryTest_SetUp()
        {
            _dbConnectionFactory = new DbConnectionFactory();
            _reportJobRepository = new ReportJobRepository(_dbConnectionFactory);


        }

        [TearDown]
        public void ReportJobRepositoryTest_TearDown()
        {
        }

        [Test]
        public void GetAverageExecutionTime_Executes_ReturnsValue()
        {
            TimeSpan result = _reportJobRepository.GetAverageExecutionTime(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), DateTime.Now.AddMonths(-3), DateTime.Now);
            Assert.GreaterOrEqual(result.TotalSeconds, 0);
        }

        [Test]
        public void GetAverageGenerationTime_Executes_ReturnsValue()
        {
            TimeSpan result = _reportJobRepository.GetAverageGenerationTime(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), DateTime.Now.AddMonths(-3), DateTime.Now);
            Assert.GreaterOrEqual(result.TotalSeconds, 0);
        }

        [Test]
        public void GetErrorCount_Executes_ReturnsValue()
        {
            int result = _reportJobRepository.GetErrorCount(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), DateTime.Now.AddMonths(-3), DateTime.Now);
            Assert.GreaterOrEqual(result, 0);
        }

        [Test]
        public void GetMostActiveUsers_Executes_WithoutSqlErrors()
        {
            _reportJobRepository.GetMostActiveUsers(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), 10, DateTime.Now.AddMonths(-3), DateTime.Now);
        }

        [Test]
        public void MostRunReports_Executes_WithoutSqlErrors()
        {
            _reportJobRepository.GetMostRunReports(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), 10, DateTime.Now.AddMonths(-3), DateTime.Now);
        }

        [Test]
        public void GetPendingReports_Executes_WithoutSqlErrors()
        {
            _reportJobRepository.GetPendingReports(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), 10);
        }

        [Test]
        public void GetReportCountByDay_Executes_WithoutSqlErrors()
        {
            _reportJobRepository.GetReportCountByDay(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), DateTime.Now.AddMonths(-3), DateTime.Now);
        }

        [Test]
        public void GetTotalReportCount_Executes_ReturnsValue()
        {
            int result = _reportJobRepository.GetTotalReportCount(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), DateTime.Now.AddMonths(-3), DateTime.Now);
            Assert.GreaterOrEqual(result, 0);
        }


    }
}
