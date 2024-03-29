﻿using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Hunarmis.Manager
{
    public class ExcelUtility
    {
        public DataTable GetData(string filePath)
        {
            var table = new DataTable();
            using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(filePath, false))
            {
                WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                string relationshipId = sheets.First().Id.Value;
                WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                Worksheet workSheet = worksheetPart.Worksheet;
                SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                IEnumerable<Row> rows = sheetData.Descendants<Row>();
                foreach (Cell cell in rows.ElementAt(0))
                {
                    table.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                }
                //this will also include your header row...
                foreach (Row row in rows)
                {
                    DataRow tempRow = table.NewRow();
                    var isRowValid = false;
                    for (int i = 0; i < row.Descendants<Cell>().Count() && i < table.Columns.Count; i++)
                    {
                        var data = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));
                        if (data != null)
                        {
                            isRowValid = true;
                        }
                        tempRow[i]  = data;
                    }
                    if (isRowValid)
                    {
                        table.Rows.Add(tempRow);
                    }
                }
            }
            table.Rows.RemoveAt(0);
            return table;
        }
        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue?.InnerXml;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
        private static int CellReferenceToIndex(Cell cell)
        {
            int index = 0;
            string reference = cell.CellReference.ToString().ToUpper();
            foreach (char ch in reference)
            {
                if (Char.IsLetter(ch))
                {
                    int value = (int)ch - (int)'A';
                    index = (index == 0) ? value : ((index + 1) * 26) + value;
                }
                else
                {
                    return index;
                }
            }
            return index;
        }


    }
}