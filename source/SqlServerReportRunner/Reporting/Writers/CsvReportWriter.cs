using CsvHelper;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class CsvReportWriter : AbstractReportWriter
    {

        private ITextFormatter _textFormatter;
        private CsvWriter _writer;

        public CsvReportWriter(ITextFormatter textFormatter)
        {
            _textFormatter = textFormatter;
        }

        public override void Initialise(string filePath, string delimiter)
        {
            this.FilePath = filePath;
            this.Delimiter = String.IsNullOrEmpty(delimiter) ? "," : delimiter;

            var writerConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = this.Delimiter
            };
            _writer = new CsvWriter(File.CreateText(this.FilePath), writerConfig, false);
        }

        public override void WriteHeader(IEnumerable<string> columnNames)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
            foreach (string col in columnNames)
            {
                _writer.WriteField(col);
            }
            _writer.NextRecord();
        }

        public override void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
            for (var i = 0; i < reader.FieldCount; i++)
            {
                _writer.WriteField(_textFormatter.FormatText(reader.GetValue(i), reader.GetFieldType(i)));
            }
            _writer.NextRecord();
        }

        public override void Dispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
            }
        }

    }
}
