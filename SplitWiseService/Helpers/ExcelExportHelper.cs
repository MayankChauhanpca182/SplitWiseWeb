using System.ComponentModel;
using System.Reflection;
using OfficeOpenXml;

namespace SplitWiseService.Helpers;

public static class ExcelExportHelper
{
    public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            using ExcelPackage package = new ExcelPackage();
            ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Get properties of the view model
            PropertyInfo[] properties = typeof(T).GetProperties();

            // Add headers
            for (int i = 0; i < properties.Length; i++)
            {
                string displayName = properties[i].GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? properties[i].Name;
                worksheet.Cells[1, i + 1].Value = displayName;
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Add data
            int row = 2; 
            foreach (T item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    object value = properties[col].GetValue(item);
                    worksheet.Cells[row, col + 1].Value = value;

                    // Format specific types
                    if (properties[col].PropertyType == typeof(DateTime) || properties[col].PropertyType == typeof(DateTime?))
                    {
                        worksheet.Cells[row, col + 1].Style.Numberformat.Format = "yyyy-mm-dd";
                    }
                    else if (properties[col].PropertyType == typeof(decimal) || properties[col].PropertyType == typeof(decimal?))
                    {
                        worksheet.Cells[row, col + 1].Style.Numberformat.Format = "#,##0.00";
                    }
                }
                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }
}
