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
        public void GetPendingReports_Executes_WithoutSqlErrors()
        {
            _reportJobRepository.GetPendingReports(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder), 10);
        }

        [Test]
        public void GetTotalReportCount_Executes_ReturnsValue()
        {
            int result = _reportJobRepository.GetTotalReportCount(TestUtility.TestDbConnectionString(TestUtility.TestRootFolder));
            Assert.GreaterOrEqual(result, 0);
        }


    }
}
