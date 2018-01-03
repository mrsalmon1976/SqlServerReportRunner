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
using SqlServerReportRunner.Constants;

namespace SqlServerReportRunner.Commands
{
    public interface IStartJobCommand 
    {
        int Execute(ConnectionSetting connection, int jobId);
        int Execute(ConnectionSetting connection, int jobId, IDbConnection dbConnection);
    }
    public class StartJobCommand : IStartJobCommand
    {
        private IConcurrencyCoordinator _concurrencyCoordinator;
        private IDbConnectionFactory _dbConnectionFactory;

        public StartJobCommand(IConcurrencyCoordinator concurrencyCoordinator, IDbConnectionFactory dbConnectionFactory)
        {
            _concurrencyCoordinator = concurrencyCoordinator;
            _dbConnectionFactory = dbConnectionFactory;
        }

        public int Execute(ConnectionSetting connection, int jobId, IDbConnection dbConnection)
        {
            const string query = "UPDATE ReportJobQueue SET ProcessStartDate = @ProcessStartDate, Status = @Status WHERE Id = @Id";

            _concurrencyCoordinator.LockReportJob(connection.Name, jobId);
            return dbConnection.Execute(query, new { Id = jobId, ProcessStartDate = DateTime.UtcNow, Status = JobStatus.Processing });
        }

        public int Execute(ConnectionSetting connection, int jobId)
        {
            using (IDbConnection dbConnection = _dbConnectionFactory.CreateConnection(connection.ConnectionString))
            {
                return this.Execute(connection, jobId, dbConnection);
            }
        }
    }
}
