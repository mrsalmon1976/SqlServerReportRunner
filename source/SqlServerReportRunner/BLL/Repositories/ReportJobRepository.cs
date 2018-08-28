using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Constants;

namespace SqlServerReportRunner.BLL.Repositories
{
    public interface IReportJobRepository
    {

        /// <summary>
        /// Gets a list of all pending reports.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IEnumerable<ReportJob> GetPendingReports(string connectionString, int count);

        /// <summary>
        /// Gets a count of all reports ever run.
        /// </summary>
        /// <returns></returns>
        int GetTotalReportCount(string connectionString);

    }

    public class ReportJobRepository : IReportJobRepository
    {
        private IDbConnectionFactory _dbConnectionFactory;

        public ReportJobRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Gets a list of all pending reports.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public IEnumerable<ReportJob> GetPendingReports(string connectionString, int count)
        {
            string query = String.Format("select TOP {0} * from ReportJobQueue WHERE [Status] = @Status AND ISNULL(ScheduleDate, '1900-01-01') <= @ScheduleDate ORDER BY Id", count);
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.Query<ReportJob>(query, new { Status = new DbString() { Value = JobStatus.Pending }, ScheduleDate = DateTime.UtcNow });
            }
        }

        /// <summary>
        /// Gets a count of all reports ever run.
        /// </summary>
        /// <returns></returns>
        public int GetTotalReportCount(string connectionString)
        {
            const string query = "SELECT COUNT(*) FROM ReportJobQueue";
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.QuerySingle<int>(query);
            }
        }

    }
}
