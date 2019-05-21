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
    public class DelimitedReportWriter : AbstractReportWriter
    {
        private ITextFormatter _textFormatter;
        private StreamWriter _writer;

        public DelimitedReportWriter(ITextFormatter textFormatter)
        {
            _textFormatter = textFormatter;
        }

        public override void Initialise(string filePath, string delimiter)
        {
            this.FilePath = filePath;
            this.Delimiter = delimiter;
            _writer = File.CreateText(this.FilePath);
        }

        public override void WriteHeader(IEnumerable<string> columnNames)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
            _writer.WriteLine(string.Join(this.Delimiter, columnNames));
        }

        public override void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");

            string[] columnValues =
                Enumerable.Range(0, columnInfo.Length)
                          .Select(i => _textFormatter.FormatText(reader.GetValue(i), reader.GetFieldType(i)))
                          .ToArray();
            _writer.WriteLine(string.Join(this.Delimiter, columnValues));
        }

        public override void Dispose()
        {
            if (_writer != null)
            {
                _writer.Close();
                _writer.Dispose();
            }
        }

    }
}
