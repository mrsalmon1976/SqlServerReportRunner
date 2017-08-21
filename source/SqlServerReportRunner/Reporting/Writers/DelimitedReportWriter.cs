using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class DelimitedReportWriter : IReportWriter
    {
        private ITextFormatter _textFormatter;
        private StreamWriter _writer;

        public DelimitedReportWriter(ITextFormatter textFormatter, string filePath)
        {
            _textFormatter = textFormatter;
            this.FilePath = filePath;
            _writer = File.CreateText(filePath);
        }

        public string FilePath { get; set; }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            _writer.WriteLine(string.Join(delimiter, columnNames));
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            string[] columnValues =
                Enumerable.Range(0, columnInfo.Length)
                          .Select(i => _textFormatter.FormatText(reader.GetValue(i), reader.GetFieldType(i)))
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

    }
}
