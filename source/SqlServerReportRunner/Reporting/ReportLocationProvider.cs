using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting
{
    public interface IReportLocationProvider
    {

        /// <summary>
        /// Gets the application root path.
        /// </summary>
        string AppPath { get; }

        /// <summary>
        /// Gets the folder that should be used to store processing data for a specific connection.
        /// </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        string GetProcessingFolder(string connectionName);
    }

    public class ReportLocationProvider : IReportLocationProvider
    {
        public string AppPath
        {
            get
            {
                return System.AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public string GetProcessingFolder(string connectionName)
        {
            return Path.Combine(this.AppPath, "Processing", connectionName);
        }
    }
}
