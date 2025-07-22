using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class SettleUpListVM
{
    public decimal TotalAmount { get; set; } = 0;
    public User CurrentUser { get; set; } = new User();
    public FriendVM Friend { get; set; } = new FriendVM();
    public List<GroupVM> Groups { get; set; } = new List<GroupVM>();
}
