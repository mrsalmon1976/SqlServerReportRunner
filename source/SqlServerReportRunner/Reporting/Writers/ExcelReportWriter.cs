using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using OfficeOpenXml;
using SqlServerReportRunner.Models;
using SqlServerReportRunner.Reporting.Executors;
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

        public ExcelReportWriter(string filePath)
        {
            this.FilePath = filePath;

            _excelPackage = new ExcelPackage();
            _workSheet = _excelPackage.Workbook.Worksheets.Add("Data");
        }

        public string FilePath { get; set; }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
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
            for (int i = 0; i < columnInfo.Length; i++)
            {
                ExcelRange range = _workSheet.Cells[_rowNum, i + 1];
                range.Value = reader.GetValue(i);
                //range.Style.Numberformat.Format = 
            }
            _rowNum++;

        }

        public void Dispose()
        {
            _excelPackage.SaveAs(new FileInfo(this.FilePath));
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
