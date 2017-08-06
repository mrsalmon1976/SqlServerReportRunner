using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using SqlServerReportRunner.Common;

namespace SqlServerReportRunner.Repositories
{
    public interface IReportJobRepository
    {

        /// <summary>
        /// Gets a list of all pending reports.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IEnumerable<ReportJob> GetPendingReports(string connectionString, int count);

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
            string query = String.Format("select TOP {0} * from ReportJobQueue WHERE Status = @Status ORDER BY Id", count);
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.Query<ReportJob>(query, new { Status = JobStatus.Pending });
            }
        }

    }
}
