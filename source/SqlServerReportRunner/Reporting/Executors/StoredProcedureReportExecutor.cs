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

namespace SqlServerReportRunner.Reporting.Executors
{
    public class StoredProcedureReportExecutor : IReportExecutor
    {
        private IDbConnectionFactory _dbConnectionFactory;

        public StoredProcedureReportExecutor(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public ReportJobResult ExecuteJob(string connectionString, ReportJob job)
        {

            using (SqlConnection conn = (SqlConnection)_dbConnectionFactory.CreateConnection(connectionString))
            {

                int rowCount = 0;
                DateTime start = DateTime.UtcNow;
                using (var command = conn.CreateCommand())
                {
                    command.CommandTimeout = 0;
                    command.CommandText = job.Command;
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddRange(new DbParameterUtility().ConvertXmlToDbParameters(job.Parameters));

                    using (var reader = command.ExecuteReader())
                    {
                        string destinationFile = Path.Combine(job.OutputFilePath, job.OutputFileName);
                        using (var fileWriter = File.CreateText(destinationFile))
                        {
                            string[] columnNames = GetColumnNames(reader).ToArray();
                            int numFields = columnNames.Length;
                            fileWriter.WriteLine(string.Join(",", columnNames));
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    string[] columnValues =
                                        Enumerable.Range(0, numFields)
                                                  .Select(i => reader.GetValue(i).ToString())
                                                  .Select(field => string.Concat("\"", field.Replace("\"", "\"\""), "\""))
                                                  .ToArray();
                                    fileWriter.WriteLine(string.Join(",", columnValues));
                                }
                            }
                            rowCount++;
                        }
                    }
                }

                ReportJobResult result = new ReportJobResult();
                result.RowCount = rowCount;
                result.ExecutionTime = start.Subtract(DateTime.UtcNow);
                return result;
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
