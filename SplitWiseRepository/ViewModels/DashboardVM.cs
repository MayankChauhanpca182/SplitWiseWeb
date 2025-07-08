using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class DashboardVM
{
    public int FriendsAccepted { get; set; } = 0;
    public int FriendsPending { get; set; } = 0;
    public int FriendsRequested { get; set; } = 0;
    public int FriendsReferred { get; set; } = 0;

    public decimal NetNonGroupExpense { get; set; } = 0;
    public decimal NetGroupExpense { get; set; } = 0;
    public decimal NetExpense { get; set; } = 0;

    public decimal TotalPaid { get; set; } = 0;
    public decimal TotalSettled { get; set; } = 0;

    public List<ExpenseVM> RecentExpenses { get; set; } = new List<ExpenseVM>();
    public List<Payment> RecentPayments { get; set; } = new List<Payment>();
}
