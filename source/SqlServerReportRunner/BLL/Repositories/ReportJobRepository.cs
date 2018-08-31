﻿using SqlServerReportRunner.Models;
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