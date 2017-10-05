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

        public StoredProcedureReportExecutor(IDbConnectionFactory dbConnectionFactory, IReportWriterFactory reportWriterFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _reportWriterFactory = reportWriterFactory;
        }

        public ReportJobResult ExecuteJob(ConnectionSetting connection, ReportJob job)
        {
            using (SqlConnection conn = (SqlConnection)_dbConnectionFactory.CreateConnection(connection.ConnectionString))
            {

                int rowCount = 0;
                DateTime start = DateTime.UtcNow;
                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = job.Command;
                    if (job.CommandType.ToLower() == CommandType.StoredProcedure)
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                    }
                    command.Parameters.AddRange(new DbParameterUtility().ConvertXmlToDbParameters(job.Parameters));

                    using (var reader = command.ExecuteReader())
                    {
                        string destinationFile = Path.Combine(job.OutputFilePath, job.OutputFileName);
                        using (IReportWriter reportWriter = _reportWriterFactory.GetReportWriter(job.OutputFormat))
                        {
                            ColumnMetaData[] columnInfo = GetColumnInfo(reader).ToArray();
                            reportWriter.Initialise(destinationFile);
                            reportWriter.WriteHeader(columnInfo.Select(x => x.Name), job.Delimiter);
                            if (reader.HasRows)
                            {
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
                result.ExecutionTime = start.Subtract(DateTime.UtcNow);
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
