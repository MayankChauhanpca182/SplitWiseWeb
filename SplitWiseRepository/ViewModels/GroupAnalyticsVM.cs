using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class GroupAnalyticsVM
{
    public decimal TotalExpense { get; set; } = 0;
    public string BaseColor { get; set; } = DefaultValues.BaseColor;
    public List<CategoryExpenseChart> CategoryExpenseChart { get; set; } = new List<CategoryExpenseChart>();
    public List<MemberExpenseChart> MemberExpenseCharts { get; set; } = new List<MemberExpenseChart>();
}

public class CategoryExpenseChart
{
    public string Category { get; set; } = string.Empty;
    public decimal Expense { get; set; } = 0;
}

public class MemberExpenseChart
{
    public string Member { get; set; } = string.Empty;
    public decimal Expense { get; set; } = 0;
}
