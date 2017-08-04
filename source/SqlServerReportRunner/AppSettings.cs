using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner
{
    public interface IAppSettings
    {
        /// <summary>
        /// Gets the user-defined connection settings defined in the configuration files.
        /// </summary>
        IEnumerable<ConnectionSetting> ConnectionSettings { get; }

        /// <summary>
        /// Gets the maximum number of reports that can run per user-defined connection.
        /// </summary>
        int MaxConcurrentReports { get; }

        /// <summary>
        /// Poll interval (in seconds) for how often the report queue is queried.
        /// </summary>
        int PollInterval { get; }

        /// <summary>
        /// Gets the account under which the current application should run.  Local System will be used if this is empty.
        /// </summary>
        string ServiceUserName { get; }

        /// <summary>
        /// Gets the password for the account under which the current application should run.
        /// </summary>
        string ServicePassword { get; }
    }

    public class AppSettings : IAppSettings
    {

        /// <summary>
        /// Gets the user-defined connection settings defined in the configuration files.
        /// </summary>
        public IEnumerable<ConnectionSetting> ConnectionSettings
        {
            get
            {
                List<ConnectionSetting> settings = new List<ConnectionSetting>();
                for (int i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
                {
                    var conn = ConfigurationManager.ConnectionStrings[i];
                    if (conn.Name == "LocalSqlServer") continue;
                    settings.Add(new ConnectionSetting(conn.Name, conn.ConnectionString));
                }
                return settings;

            }
        }

        /// <summary>
        /// Gets the maximum number of reports that can run per user-defined connection.
        /// </summary>
        public int MaxConcurrentReports
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["MaxConcurrentReports"]) * 1000;
                }
                catch (Exception ex)
                {
                    throw new ConfigurationErrorsException("Application setting 'MaxConcurrentReports' is missing or not a valid integer.", ex);
                }
            }
        }

        /// <summary>
        /// Poll interval (in seconds) for how often the report queue is queried.
        /// </summary>
        public int PollInterval
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["PollInterval"]) * 1000;
                }
                catch (Exception ex)
                {
                    throw new ConfigurationErrorsException("Application setting 'PollInterval' is missing or not a valid integer.", ex);
                }
            }
        }

        /// <summary>
        /// Gets the account under which the current application should run.  Local System will be used if this is empty.
        /// </summary>
        public string ServiceUserName
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["ServiceUserName"];
                }
                catch (Exception ex)
                {
                    throw new ConfigurationErrorsException("Application setting 'ServiceUserName' is missing.", ex);
                }
            }
        }

        /// <summary>
        /// Gets the password for the account under which the current application should run.
        /// </summary>
        public string ServicePassword
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["ServicePassword"];
                }
                catch (Exception ex)
                {
                    throw new ConfigurationErrorsException("Application setting 'ServicePassword' is missing.", ex);
                }
            }
        }

    }
}
