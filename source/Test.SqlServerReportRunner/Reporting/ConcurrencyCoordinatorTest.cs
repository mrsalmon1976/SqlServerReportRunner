using NSubstitute;
using NUnit.Framework;
using SqlServerReportRunner.Reporting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.SqlServerReportRunner.Reporting
{
    [TestFixture]
    public class ConcurrencyCoordinatorTest
    {
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IReportLocationProvider _reportLocationProvider;
        private string _testRootFolder;

        [SetUp]
        public void ConcurrencyCoordinatorTest_SetUp()
        {
            _testRootFolder = Path.Combine(Environment.CurrentDirectory, "ConcurrencyCoordinatorTest");
            Directory.CreateDirectory(_testRootFolder);

            _reportLocationProvider = Substitute.For<IReportLocationProvider>();

            _concurrencyCoordinator = new ConcurrencyCoordinator(_reportLocationProvider);
        }

        public void ConcurrencyCoordinatorTest_TearDown()
        {
            if (Directory.Exists(_testRootFolder))
            {
                Directory.Delete(_testRootFolder, true);
            }
        }


        [Test]
        public void GetRunningReportCount_NoFiles_ReturnsZero()
        {
            // setup
            string connectionName = Path.GetRandomFileName();
            string processingFolder = Path.Combine(Environment.CurrentDirectory, connectionName);
            _reportLocationProvider.GetProcessingFolder(connectionName).Returns(processingFolder);

            // execute
            int result = _concurrencyCoordinator.GetRunningReportCount(connectionName);

            // assert
            Assert.AreEqual(0, result);

        }

        [Test]
        public void GetRunningReportCount_ContainsFiles_ReturnsCorrectCount()
        {
            // setup
            string connectionName = Path.GetRandomFileName();
            string processingFolder = Path.Combine(Environment.CurrentDirectory, connectionName);
            _reportLocationProvider.GetProcessingFolder(connectionName).Returns(processingFolder);

            Directory.CreateDirectory(processingFolder);
            int fileCount = new Random().Next(1, 7);
            for (int i=0; i<fileCount; i++)
            {
                string filePath = Path.Combine(processingFolder, i.ToString());
                File.WriteAllText(filePath, "test data");
            }

            // execute
            int result = _concurrencyCoordinator.GetRunningReportCount(connectionName);

            // assert
            Assert.AreEqual(fileCount, result);

        }

    }
}
