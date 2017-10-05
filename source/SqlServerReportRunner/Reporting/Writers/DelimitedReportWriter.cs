﻿using SqlServerReportRunner.Models;
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
        private string _filePath;

        public DelimitedReportWriter(ITextFormatter textFormatter)
        {
            _textFormatter = textFormatter;
        }

        public void Initialise(string filePath)
        {
            _filePath = filePath;
            _writer = File.CreateText(_filePath);
        }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");
            _writer.WriteLine(string.Join(delimiter, columnNames));
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            if (_writer == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");

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
