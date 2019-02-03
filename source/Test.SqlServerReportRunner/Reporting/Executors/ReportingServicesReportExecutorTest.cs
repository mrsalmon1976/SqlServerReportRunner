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
using SqlServerReportRunner.BLL.Net;
using System.IO;

namespace Test.SqlServerReportRunner.Reporting.Executors
{
    [TestFixture]
    public class ReportingServicesReportExecutorTest
    {
        private IReportExecutor _reportExecutor;

        private IWebClientWrapper _webClientWrapper;

        [SetUp]
        public void ReportingServicesReportExecutorTest_SetUp()
        {

            _webClientWrapper = Substitute.For<IWebClientWrapper>();

            _reportExecutor = new ReportingServicesReportExecutor(_webClientWrapper);
        }

        [Test]
        public void ExecuteJob_OnExecute_DownloadsFile()
        {
            // setup
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.OutputFilePath = Environment.CurrentDirectory;
            job.OutputFileName = Path.GetRandomFileName();

            string expectedOutPath = Path.Combine(job.OutputFilePath, job.OutputFileName);

            // execute
            _reportExecutor.ExecuteJob(null, job);

            // assert
            _webClientWrapper.Received(1).UseDefaultCredentials = true;
            _webClientWrapper.Received(1).DownloadFile(job.Command, expectedOutPath);

        }

        [Test]
        public void ExecuteJob_OnExecute_ResultIsValid()
        {
            // setup
            ReportJob job = new ReportJob();
            job.Command = Guid.NewGuid().ToString();
            job.OutputFilePath = Environment.CurrentDirectory;
            job.OutputFileName = Path.GetRandomFileName();

            string expectedOutPath = Path.Combine(job.OutputFilePath, job.OutputFileName);

            // execute
            ReportJobResult result = _reportExecutor.ExecuteJob(null, job);

            // assert
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result.RowCount);
            Assert.GreaterOrEqual(result.ExecutionTime.Milliseconds, 0);

        }

    }
}
