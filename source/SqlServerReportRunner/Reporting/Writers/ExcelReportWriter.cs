using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class ExcelReportWriter : IReportWriter
    {

        private XLWorkbook _spreadSheet;
        private IXLWorksheet _workSheet;

        public ExcelReportWriter(string filePath)
        {
            this.FilePath = filePath;

            _spreadSheet = new XLWorkbook();
            _workSheet = _spreadSheet.Worksheets.Add("Data");
        }

        public string FilePath { get; set; }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            string[] columns = columnNames.ToArray();
            for (int i = 0; i < columns.Length; i++)
            {
                var cell = _workSheet.Cell(1, i + 1);
                cell.Value = columns[i];
                cell.Style.Font.Bold = true;
            }
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            var lastRowUsed = _workSheet.LastRowUsed();
            int row = (lastRowUsed == null ? 1 : lastRowUsed.RowNumber() + 1);

            for (int i = 0; i < columnInfo.Length; i++ )
            {
                _workSheet.Cell(row, i + 1).Value = FormatData(reader.GetValue(i));
            }
        }

        public void Dispose()
        {
            _spreadSheet.SaveAs(this.FilePath);
            _spreadSheet.Dispose();
        }

        private object FormatData(object itemValue)
        {
            if (itemValue is DBNull || itemValue == null)
            {
                return String.Empty;
            }
            return itemValue;
        }

    }
}
