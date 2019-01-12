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
        /// Gets the average execution time of the reports (from when it starts processing to finish time).
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        TimeSpan GetAverageExecutionTime(string connectionString, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets the average generation time of the reports (from create date to finish time).
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        TimeSpan GetAverageGenerationTime(string connectionString, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a list of the most active users with a specified time frame.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="count">The number of users to return</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns></returns>
        IEnumerable<ReportCount> GetMostActiveUsers(string connectionString, int count, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets a list of all pending reports.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        IEnumerable<ReportJob> GetPendingReports(string connectionString, int count);

        /// <summary>
        /// Gets the number of reports run per day for a specified time frame.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns></returns>
        IEnumerable<ReportCount> GetReportCountByDay(string connectionString, DateTime startDate, DateTime endDate);


        /// <summary>
        /// Gets a count of all reports ever run.
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        int GetTotalReportCount(string connectionString, DateTime startDate, DateTime endDate);

    }

    public class ReportJobRepository : IReportJobRepository
    {
        private IDbConnectionFactory _dbConnectionFactory;

        public ReportJobRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        /// <summary>
        /// Gets the average execution time of the reports (from when it starts processing to finish time).
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        public TimeSpan GetAverageExecutionTime(string connectionString, DateTime startDate, DateTime endDate)
        {
            const string query = @"SELECT ISNULL(AVG(DATEDIFF(second, ProcessStartDate, ProcessEndDate)), 0)
                FROM ReportJobQueue
                WHERE Status = 'Complete'
                AND CreateDate >= @StartDate 
                AND CreateDate < @EndDate";
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                int seconds = conn.QuerySingle<int>(query, new { StartDate = startDate, EndDate = endDate });
                return TimeSpan.FromSeconds(seconds);
            }
        }

        /// <summary>
        /// Gets the average generation time of the reports (from create date to finish time).
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        public TimeSpan GetAverageGenerationTime(string connectionString, DateTime startDate, DateTime endDate)
        {
            const string query = @"SELECT ISNULL(AVG(DATEDIFF(second, CreateDate, ProcessEndDate)), 0)
                FROM ReportJobQueue
                WHERE Status = 'Complete'
                AND CreateDate >= @StartDate 
                AND CreateDate < @EndDate";
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                int seconds = conn.QuerySingle<int>(query, new { StartDate = startDate, EndDate = endDate });
                return TimeSpan.FromSeconds(seconds);
            }
        }

        /// <summary>
        /// Gets a list of the most active users with a specified time frame.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="count">The number of users to return</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns></returns>
        public IEnumerable<ReportCount> GetMostActiveUsers(string connectionString, int count, DateTime startDate, DateTime endDate)
        {
            string query = String.Format(@"SELECT TOP {0} UserName AS [Key], COUNT(Id) AS [Count] 
                FROM ReportJobQueue
                WHERE CreateDate >= @StartDate 
                AND CreateDate < @EndDate
                GROUP BY UserName
                ORDER BY COUNT(Id) DESC", count);
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.Query<ReportCount>(query, new { StartDate = startDate, EndDate = endDate });
            }
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
        /// Gets the number of reports run per day for a specified time frame.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns></returns>
        public IEnumerable<ReportCount> GetReportCountByDay(string connectionString, DateTime startDate, DateTime endDate)
        {
            const string query = @"SELECT CAST(CAST(CreateDate AS date) AS varchar(10)) AS [Key], COUNT(Id) AS [Count] 
                FROM ReportJobQueue
                WHERE CreateDate >= @StartDate 
                AND CreateDate < @EndDate
                GROUP BY CAST(CreateDate AS date)
                ORDER BY 1";
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.Query<ReportCount>(query, new { StartDate = startDate, EndDate = endDate });
            }
        }

        /// <summary>
        /// Gets a count of all reports ever run.
        /// </summary>
        /// <param name="connectionString">The connection string to use to fetch the data.</param>
        /// <param name="startDate">The date from which to include reports (greater than equal to).</param>
        /// <param name="endDate">The date until which to include reports (less than and NOT equal to).</param>
        /// <returns></returns>
        public int GetTotalReportCount(string connectionString, DateTime startDate, DateTime endDate)
        {
            const string query = "SELECT COUNT(*) FROM ReportJobQueue WHERE CreateDate >= @StartDate AND CreateDate < @EndDate";
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connectionString))
            {
                return conn.QuerySingle<int>(query, new { StartDate = startDate, EndDate = endDate });
            }
        }

    }
}
