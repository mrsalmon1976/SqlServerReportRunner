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
        private string _filePath;

        public CsvReportWriter(ITextFormatter textFormatter)
        {
            _textFormatter = textFormatter;
        }

        public void Initialise(string filePath)
        {
            _filePath = filePath;
            _writer = new CsvWriter(File.CreateText(this._filePath));
        }
        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
            foreach (string col in columnNames)
            {
                _writer.WriteField(col);
            }
            _writer.NextRecord();
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
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
