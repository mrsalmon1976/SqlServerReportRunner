using CsvHelper;
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
    public class CsvReportWriter : IReportWriter
    {

        private ITextFormatter _textFormatter;
        private CsvWriter _writer;

        public CsvReportWriter(ITextFormatter textFormatter, string filePath)
        {
            _textFormatter = textFormatter;
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
                _writer.WriteField(_textFormatter.FormatText(reader.GetValue(i), reader.GetFieldType(i)));
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

    }
}
