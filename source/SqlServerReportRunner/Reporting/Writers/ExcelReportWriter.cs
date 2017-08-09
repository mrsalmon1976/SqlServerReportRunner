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

        private SpreadsheetDocument _spreadSheet;
        private Worksheet _workSheet;

        public ExcelReportWriter(string filePath)
        {
            this.FilePath = filePath;

            // create the excel document
            _spreadSheet = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
            _spreadSheet.AddWorkbookPart();
            _spreadSheet.WorkbookPart.Workbook = new Workbook();
            _spreadSheet.WorkbookPart.AddNewPart<WorksheetPart>();

            _spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet = new Worksheet();
            _spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.AppendChild(new SheetData());

        }

        public string FilePath { get; set; }

        public void WriteHeader(IEnumerable<string> columnNames, string delimiter)
        {
            Row headerRow = new Row();

            foreach (string col in columnNames)
            {
                Cell cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(col);
                headerRow.AppendChild(cell);
            }

            _spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(headerRow);


        }

        public void WriteLine(IDataReader reader, ColumnMetaData[] columnInfo, string delimiter)
        {
            Row newRow = new Row();

            for (int i = 0; i < columnInfo.Length; i++ )
            {
                Cell cell = new Cell();
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(FormatData(reader.GetValue(i)));
                newRow.AppendChild(cell);
            }
            _spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.First().AppendChild(newRow);
        }

        public void Dispose()
        {
            if (_spreadSheet != null)
            {
                _spreadSheet.WorkbookPart.WorksheetParts.First().Worksheet.Save();

                // create the worksheet to workbook relation
                _spreadSheet.WorkbookPart.Workbook.AppendChild(new Sheets());
                _spreadSheet.WorkbookPart.Workbook.GetFirstChild<Sheets>().AppendChild(new Sheet()
                {
                    Id = _spreadSheet.WorkbookPart.GetIdOfPart(_spreadSheet.WorkbookPart.WorksheetParts.First()),
                    SheetId = 1,
                    Name = "Data"
                });

                _spreadSheet.WorkbookPart.Workbook.Save(); _spreadSheet.Dispose();
            }
        }

        private string FormatData(object itemValue)
        {
            if (itemValue is DBNull || itemValue == null)
            {
                return String.Empty;
            }
            return itemValue.ToString();
        }

        private void ExportDataSet(DataSet ds, string destination)
        {
            using (var workbook = SpreadsheetDocument.Create(destination, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook))
            {
                var workbookPart = workbook.AddWorkbookPart();

                workbook.WorkbookPart.Workbook = new DocumentFormat.OpenXml.Spreadsheet.Workbook();

                workbook.WorkbookPart.Workbook.Sheets = new DocumentFormat.OpenXml.Spreadsheet.Sheets();

                foreach (System.Data.DataTable table in ds.Tables)
                {

                    var sheetPart = workbook.WorkbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new DocumentFormat.OpenXml.Spreadsheet.SheetData();
                    sheetPart.Worksheet = new DocumentFormat.OpenXml.Spreadsheet.Worksheet(sheetData);

                    DocumentFormat.OpenXml.Spreadsheet.Sheets sheets = workbook.WorkbookPart.Workbook.GetFirstChild<DocumentFormat.OpenXml.Spreadsheet.Sheets>();
                    string relationshipId = workbook.WorkbookPart.GetIdOfPart(sheetPart);

                    uint sheetId = 1;
                    if (sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Count() > 0)
                    {
                        sheetId =
                            sheets.Elements<DocumentFormat.OpenXml.Spreadsheet.Sheet>().Select(s => s.SheetId.Value).Max() + 1;
                    }

                    DocumentFormat.OpenXml.Spreadsheet.Sheet sheet = new DocumentFormat.OpenXml.Spreadsheet.Sheet() { Id = relationshipId, SheetId = sheetId, Name = table.TableName };
                    sheets.Append(sheet);

                    DocumentFormat.OpenXml.Spreadsheet.Row headerRow = new DocumentFormat.OpenXml.Spreadsheet.Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                        cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                        cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }


                    sheetData.AppendChild(headerRow);

                    foreach (System.Data.DataRow dsrow in table.Rows)
                    {
                        DocumentFormat.OpenXml.Spreadsheet.Row newRow = new DocumentFormat.OpenXml.Spreadsheet.Row();
                        foreach (String col in columns)
                        {
                            DocumentFormat.OpenXml.Spreadsheet.Cell cell = new DocumentFormat.OpenXml.Spreadsheet.Cell();
                            cell.DataType = DocumentFormat.OpenXml.Spreadsheet.CellValues.String;
                            cell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue(dsrow[col].ToString()); //
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                }
            }
        }

    }
}
