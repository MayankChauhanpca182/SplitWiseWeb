namespace SplitWiseRepository.Attributes;

public class ExcelColumnAttribute : Attribute
{
    public string ColumnName { get; }

    public ExcelColumnAttribute(string columnName)
    {
        ColumnName = columnName;
    }
}
