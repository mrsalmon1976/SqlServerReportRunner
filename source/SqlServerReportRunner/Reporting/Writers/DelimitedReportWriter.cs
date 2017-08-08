using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class DelimitedReportWriter : IReportWriter
    {

        private StreamWriter _writer;

        public void CreateFile(string filePath)
        {
            _writer = File.CreateText(filePath);
        }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            ValidateWriter();
            _writer.WriteLine(string.Join(delimiter, columnNames));
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            ValidateWriter();
            string[] columnValues =
                Enumerable.Range(0, columnInfo.Length)
                          .Select(i => FormatData(reader.GetValue(i)))
                          //.Select(field => string.Concat("\"", field.Replace("\"", "\"\""), "\""))
                          .ToArray();
            _writer.WriteLine(string.Join(delimiter, columnValues));
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
            }
        }

        private void ValidateWriter()
        {
            if (_writer == null)
            {
                throw new InvalidOperationException("StreamWriter is null - CreateFile must be called before WriteHeader or WriteLine");
            }
        }

        private string FormatData(object itemValue)
        {
            if (itemValue is DBNull || itemValue == null)
            {
                return String.Empty;
            }
            return itemValue.ToString().Replace("\n", "").Replace("\r", "");
        }

    }
}
