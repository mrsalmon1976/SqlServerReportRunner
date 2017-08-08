using CsvHelper;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class CsvReportWriter : IReportWriter
    {

        private CsvWriter _writer;

        public CsvReportWriter(string filePath)
        {
            this.FilePath = filePath;
            _writer = new CsvWriter(File.CreateText(filePath));
        }

        public string FilePath { get; set; }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            foreach (string col in columnNames)
            {
                _writer.WriteField(col);
            }
            _writer.NextRecord();
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                _writer.WriteField(FormatData(reader.GetValue(i)));
            }
            _writer.NextRecord();
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
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
