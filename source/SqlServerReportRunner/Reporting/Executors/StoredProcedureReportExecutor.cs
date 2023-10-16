using SqlServerReportRunner.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.IO;
using SqlServerReportRunner.Common;
using SqlServerReportRunner.Reporting.Writers;
using NLog;

namespace SqlServerReportRunner.Reporting.Executors
{
    public class StoredProcedureReportExecutor : IReportExecutor
    {
        private IDbConnectionFactory _dbConnectionFactory;
        private IReportWriterFactory _reportWriterFactory;
        private IDbParameterUtility _dbParameterUtility;

        private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

        public StoredProcedureReportExecutor(IDbConnectionFactory dbConnectionFactory, IReportWriterFactory reportWriterFactory, IDbParameterUtility dbParameterUtility)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _reportWriterFactory = reportWriterFactory;
            _dbParameterUtility = dbParameterUtility;
        }

        public ReportJobResult ExecuteJob(ConnectionSetting connection, ReportJob job)
        {
            using (IDbConnection conn = _dbConnectionFactory.CreateConnection(connection.ConnectionString))
            {

                int rowCount = 0;
                DateTime start = DateTime.UtcNow;
                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = job.Command;
                    if (!String.IsNullOrEmpty(job.CommandType) && job.CommandType.ToLower() == Constants.CommandType.StoredProcedure)
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }

                    // add the parameters to the command
                    SqlParameter[] parameters = _dbParameterUtility.ConvertXmlToDbParameters(job.Parameters);
                    foreach (IDataParameter p in parameters)
                    {
                        command.Parameters.Add(p);
                    }

                    // make sure an output format has been specified - if it hasn't then just execute the command
                    if (String.IsNullOrEmpty(job.OutputFormat))
                    {
                        _logger.Info($"No output format specified - executing command text {job.Command} (Job {job.Id})");
                        command.ExecuteNonQuery();
                        _logger.Info($"Completed execution of command text {job.Command} (Job {job.Id})");
                    }
                    else
                    {
                        _logger.Info($"Executing command text {job.Command} (Job {job.Id})");
                        using (var reader = command.ExecuteReader())
                        {
                            string destinationFile = Path.Combine(job.OutputFilePath, job.OutputFileName);
                            _logger.Debug($"Initialising report writer for (Job {job.Id})");
                            using (IReportWriter reportWriter = _reportWriterFactory.GetReportWriter(job.OutputFormat))
                            {
                                _logger.Debug($"Report writer created (Job {job.Id})");
                                ColumnMetaData[] columnInfo = GetColumnInfo(reader).ToArray();
                                reportWriter.Initialise(destinationFile, job.Delimiter);
                                _logger.Debug($"Report writer initialised (Job {job.Id})");
                                reportWriter.WriteHeader(columnInfo.Select(x => x.Name));
                                _logger.Debug($"Report header written (Job {job.Id})");
                                while (reader.Read())
                                {
                                    reportWriter.WriteLine(reader, columnInfo);
                                    rowCount++;
                                }
                            }
                        }
                        _logger.Info($"Completed execution of command text {job.Command} (Job {job.Id})");
                    }
                }

                ReportJobResult result = new ReportJobResult();
                result.RowCount = rowCount;
                result.ExecutionTime = DateTime.UtcNow.Subtract(start);
                return result;
            }
        }

        private IEnumerable<ColumnMetaData> GetColumnInfo(IDataReader reader)
        {
            foreach (DataRow row in reader.GetSchemaTable().Rows)
            {
                ColumnMetaData metaData = new ColumnMetaData();
                metaData.Name = (string)row["ColumnName"];
                metaData.Size = (int)row["ColumnSize"];
                metaData.DataType = (string)row["DataTypeName"];
                yield return metaData;
            }
        }

        private IEnumerable<string> GetColumnNames(IDataReader reader)
        {
            foreach (DataRow row in reader.GetSchemaTable().Rows)
            {
                yield return (string)row["ColumnName"];
            }
        }
    }
}
