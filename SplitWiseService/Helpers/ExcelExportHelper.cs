using System.ComponentModel;
using System.Reflection;
using OfficeOpenXml;

namespace SplitWiseService.Helpers;

public static class ExcelExportHelper
{
    public static byte[] ExportToExcel<T>(IEnumerable<T> data, List<string> columns, string sheetName = "Sheet1")
    {
        using ExcelPackage package = new ExcelPackage();
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);

        // Get properties of the view model
        PropertyInfo[] properties = typeof(T).GetProperties();

        // Add headers
        int col = 0;
        for (int i = 0; i < properties.Length; i++)
        {
            if (columns.Contains(properties[i].Name))
            {
                string displayName = properties[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? properties[i].Name;
                worksheet.Cells[1, col + 1].Value = displayName;
                worksheet.Cells[1, col + 1].Style.Font.Bold = true;
                col++;
            }
        }

        // Add data
        int row = 2;
        foreach (T item in data)
        {
            col = 0;
            for (int i = 0; i < properties.Length; i++)
            {
                if (columns.Contains(properties[i].Name))
                {

                    object value = properties[i].GetValue(item);
                    worksheet.Cells[row, col + 1].Value = value;

                    // Format specific types
                    if (properties[i].PropertyType == typeof(DateTime) || properties[i].PropertyType == typeof(DateTime?))
                    {
                        worksheet.Cells[row, col + 1].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                    else if (properties[i].PropertyType == typeof(decimal) || properties[i].PropertyType == typeof(decimal?))
                    {
                        worksheet.Cells[row, col + 1].Style.Numberformat.Format = "#,##0.00";
                    }
                    
                    col++;
                }
            }
            row++;
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}
