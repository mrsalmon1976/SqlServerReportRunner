using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting
{
    public interface IConcurrencyCoordinator
    {
        int GetRunningReportCount(string connectionName);

        void LockReportJob(string connectionName, int jobId);
    }

    public class ConcurrencyCoordinator : IConcurrencyCoordinator
    {
        private IReportLocationProvider _reportLocationProvider;

        public ConcurrencyCoordinator(IReportLocationProvider reportLocationProvider)
        {
            _reportLocationProvider = reportLocationProvider;
        }

        public int GetRunningReportCount(string connectionName)
        {
            string path = _reportLocationProvider.GetProcessingFolder(connectionName);
            Directory.CreateDirectory(path);
            string[] files = Directory.GetFiles(path);
            return files.Length;
        }

        public void LockReportJob(string connectionName, int jobId)
        {
            throw new NotImplementedException();
        }
    }
}
