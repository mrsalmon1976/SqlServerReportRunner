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
        int[] GetRunningReports(string connectionName);

        void ClearAllLocks();

        void LockReportJob(string connectionName, int jobId);

        void UnlockReportJob(string connectionName, int jobId);
    }

    public class ConcurrencyCoordinator : IConcurrencyCoordinator
    {
        private IReportLocationProvider _reportLocationProvider;

        public ConcurrencyCoordinator(IReportLocationProvider reportLocationProvider)
        {
            _reportLocationProvider = reportLocationProvider;
        }

        public void ClearAllLocks()
        {
            string path = _reportLocationProvider.GetProcessingRootFolder();
            Directory.CreateDirectory(path);

            string[] directories = Directory.GetDirectories(path);
            foreach (string dir in directories)
            {
                Directory.Delete(dir, true);
            }

        }

        public int[] GetRunningReports(string connectionName)
        {
            string path = _reportLocationProvider.GetProcessingFolder(connectionName);
            Directory.CreateDirectory(path);
            string[] files = Directory.GetFiles(path);
            List<int> runningReports = new List<int>();
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                int jobId;
                if (Int32.TryParse(fileName, out jobId))
                {
                    runningReports.Add(jobId);
                }
            }
            return runningReports.ToArray();
        }

        public void LockReportJob(string connectionName, int jobId)
        {
            string path = _reportLocationProvider.GetProcessingFolder(connectionName);
            Directory.CreateDirectory(path);
            string filePath = Path.Combine(path, jobId.ToString());
            File.WriteAllText(filePath, String.Empty);
        }

        public void UnlockReportJob(string connectionName, int jobId)
        {
            string path = _reportLocationProvider.GetProcessingFolder(connectionName);
            Directory.CreateDirectory(path);
            string filePath = Path.Combine(path, jobId.ToString());
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

    }
}
