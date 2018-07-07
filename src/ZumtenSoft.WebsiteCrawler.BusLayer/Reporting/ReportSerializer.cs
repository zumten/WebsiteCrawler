using System;
using System.Data;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace ZumtenSoft.WebsiteCrawler.BusLayer.Reporting
{
    public static class ReportSerializer
    {
        public static void AddReport(ExcelPackage package, DataTable table)
        {
            const int EXCEL_MAX_ROWS = 1048575;
            int nbTables = ((table.Rows.Count - 1)/EXCEL_MAX_ROWS) + 1;
            for (int tableIndex = 0; tableIndex < nbTables; tableIndex ++)
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(table.TableName + (tableIndex == 0 ? "" : " (" + tableIndex + ")"));

                for (int indexColumn = 0; indexColumn < table.Columns.Count; indexColumn++)
                {
                    DataColumn column = table.Columns[indexColumn];
                    ExcelColumn excelColumn = worksheet.Column(indexColumn + 1);

                    worksheet.Cells[1, indexColumn + 1].Value = column.ColumnName;

                    double? width = column.ExtendedProperties["Width"] as double?;
                    string format = column.ExtendedProperties["Format"] as string;
                    if (width.HasValue)
                        excelColumn.Width = width.Value;

                    if (format != null)
                        excelColumn.Style.Numberformat.Format = format;
                }

                int minRow = tableIndex * EXCEL_MAX_ROWS;
                int countRows = Math.Min(table.Rows.Count - minRow, EXCEL_MAX_ROWS);
                for (int indexRow = 0; indexRow < countRows; indexRow++)
                {
                    DataRow row = table.Rows[minRow + indexRow];
                    for (int indexColumn = 0; indexColumn < table.Columns.Count; indexColumn++)
                    {
                        DataColumn column = table.Columns[indexColumn];
                        worksheet.Cells[indexRow + 2, indexColumn + 1].Value = row[column.ColumnName];
                    }
                }

                if (table.Columns.Count > 0 && table.Rows.Count > 0)
                {
                    ExcelTable excelTable = worksheet.Tables.Add(worksheet.Cells[1, 1, countRows + 1, table.Columns.Count], table.TableName + "_Table_" + tableIndex);
                    excelTable.TableStyle = TableStyles.Medium2;
                }
            }
        }
    }
}
