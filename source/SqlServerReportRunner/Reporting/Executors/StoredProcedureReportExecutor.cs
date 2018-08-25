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

namespace SqlServerReportRunner.Reporting.Executors
{
    public class StoredProcedureReportExecutor : IReportExecutor
    {
        private IDbConnectionFactory _dbConnectionFactory;
        private IReportWriterFactory _reportWriterFactory;
        private IDbParameterUtility _dbParameterUtility;

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
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            string destinationFile = Path.Combine(job.OutputFilePath, job.OutputFileName);
                            using (IReportWriter reportWriter = _reportWriterFactory.GetReportWriter(job.OutputFormat))
                            {
                                ColumnMetaData[] columnInfo = GetColumnInfo(reader).ToArray();
                                reportWriter.Initialise(destinationFile);
                                reportWriter.WriteHeader(columnInfo.Select(x => x.Name), job.Delimiter);
                                while (reader.Read())
                                {
                                    reportWriter.WriteLine(reader, columnInfo, job.Delimiter);
                                    rowCount++;
                                }
                            }
                        }
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
