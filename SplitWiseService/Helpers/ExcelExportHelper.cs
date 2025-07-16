using System.ComponentModel;
using System.Reflection;
using OfficeOpenXml;
using SplitWiseRepository.Attributes;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Helpers;

public static class ExcelExportHelper
{
    public static byte[] ExportToExcel<T>(List<T> data, FilterVM filter, string sheetName = "Sheet1")
    {
        using ExcelPackage package = new ExcelPackage();
        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetName);

        Type type = typeof(T);

        // Get properties with ExcelColumnAttribute
        List<PropertyInfo> propsWithAttribute = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ExcelColumnAttribute>() != null)
                .ToList();

        // Add filter details
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? string.Empty : filter.SearchString.Replace(" ", "").ToLower();

        string filterDetail = "Result was filtered"
                            + (filter.FromDate.HasValue && filter.ToDate.HasValue ? $" from {filter.FromDate?.ToString("dd-MM-yyyy")} to {filter.ToDate?.ToString("dd-MM-yyyy")}" : "")
                            + (string.IsNullOrEmpty(searchString) ? " without search query." : $" with search query {searchString}");

        // Add to excel
        worksheet.Cells[1, 1, 1, propsWithAttribute.Count].Merge = true;
        worksheet.Cells[1, 1].Value = filterDetail;

        // Add headers
        for (int col = 0; col < propsWithAttribute.Count; col++)
        {
            ExcelColumnAttribute attr = propsWithAttribute[col].GetCustomAttribute<ExcelColumnAttribute>();
            string columnName = attr?.ColumnName ?? propsWithAttribute[col].Name;
            worksheet.Cells[3, col + 1].Value = columnName;
            worksheet.Cells[3, col + 1].Style.Font.Bold = true;
        }

        // Add rows
        for (int row = 0; row < data.Count(); row++)
        {
            T item = data[row];
            for (int col = 0; col < propsWithAttribute.Count; col++)
            {
                object value = propsWithAttribute[col].GetValue(item);
                worksheet.Cells[row + 4, col + 1].Value = value;
            }
        }

        // Auto-fit columns
        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}
