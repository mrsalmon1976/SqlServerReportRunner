using OfficeOpenXml;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
using SqlServerReportRunner.Reporting.Formatters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;

namespace SqlServerReportRunner.Reporting.Writers
{
    public class ExcelReportWriter : IReportWriter
    {

        private int _rowNum = 1;
        private ExcelPackage _excelPackage;
        private ExcelWorksheet _workSheet;
        private IExcelRangeFormatter _excelRangeFormatter;
        private string _filePath;

        public ExcelReportWriter(IExcelRangeFormatter excelRangeFormatter)
        {
            _excelRangeFormatter = excelRangeFormatter;
        }

        public void Initialise(string filePath)
        {
            _filePath = filePath;
            _excelPackage = new ExcelPackage();
            _workSheet = _excelPackage.Workbook.Worksheets.Add("Data");
        }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            if (_excelPackage == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");

            int colIndex = 1;
            foreach (string col in columnNames)
            {
                ExcelRange range = _workSheet.Cells[_rowNum, colIndex];
                range.Style.Font.Bold = true;
                range.Value = col;
                colIndex++;
            }
            _rowNum++;
        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            if (_excelPackage == null) throw new InvalidOperationException("Initialise must be called to initialise the report writer");

            for (int i = 0; i < columnInfo.Length; i++)
            {
                ExcelRange range = _workSheet.Cells[_rowNum, i + 1];
                object cellValue = reader.GetValue(i);
                Type dataType = reader.GetFieldType(i);
                _excelRangeFormatter.FormatCell(range, cellValue, dataType);
            }
            _rowNum++;

        }

        public void Dispose()
        {
            _excelPackage.SaveAs(new FileInfo(this._filePath));
            _workSheet.Dispose();
            _excelPackage.Dispose();
            _excelPackage = null;
        }

    }
}
