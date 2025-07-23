namespace SplitWiseRepository.ViewModels;

public class FriendAnalyticsVM
{
    public decimal TotalExpense { get; set; } = 0;
    public List<CategoryExpenseChart> CategoryExpenseChart { get; set; } = new List<CategoryExpenseChart>();
    public List<CategoryExpenseChart> GroupTypeExpenseCharts { get; set; } = new List<CategoryExpenseChart>();
}
