namespace SplitWiseRepository.ViewModels;

public class DashboardVM
{
    public int FriendsAccepted { get; set; } = 0;
    public int FriendsPending { get; set; } = 0;
    public int FriendsRequested { get; set; } = 0;
    public int FriendsReferred { get; set; } = 0;

    public int GroupsCount { get; set; } = 0;
    public decimal TotalGroupExpense { get; set; } = 0;


    public decimal TotalExpense { get; set; } = 0;
    public List<ExpenseVM> RecentExpenses { get; set; } = new List<ExpenseVM>();
}
