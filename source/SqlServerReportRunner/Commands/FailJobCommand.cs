using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Reporting;
using SqlServerReportRunner.Models;

namespace SqlServerReportRunner.Commands
{
    public interface IFailJobCommand
    {
        int Execute(ConnectionSetting connection, int jobId, Exception ex);
        int Execute(ConnectionSetting connection, int jobId, Exception ex, IDbConnection dbConnection);
    }
    public class FailJobCommand : IFailJobCommand
    {
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IDbConnectionFactory _dbConnectionFactory;
        
        public FailJobCommand(IConcurrencyCoordinator concurrencyCoordinator, IDbConnectionFactory dbConnectionFactory)
        {
            _concurrencyCoordinator = concurrencyCoordinator;
            _dbConnectionFactory = dbConnectionFactory;
        }

        public int Execute(ConnectionSetting connection, int jobId, Exception ex, IDbConnection dbConnection)
        {
            const string query = "UPDATE ReportJobQueue SET Status = @Status, ErrorMessage = @ErrorMessage, ErrorStackTrace = @ErrorStackTrace WHERE Id = @Id";
            _concurrencyCoordinator.UnlockReportJob(connection.Name, jobId);
            return dbConnection.Execute(query, new { Id = jobId, Status = JobStatus.Error, ErrorMessage = ex.Message, ErrorStackTrace = ex.ToString()});
        }

        public int Execute(ConnectionSetting connection, int jobId, Exception ex)
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection(connection.ConnectionString))
            {
                return this.Execute(connection, jobId, ex, dbConnection);
            }
        }
    }
}
