namespace SplitWiseRepository.ViewModels;

public class FriendRequestListVM
{
    public List<FriendRequestVM> FriendRequestList { get; set; } = new List<FriendRequestVM>();
    public PaginationVM Page { get; set; } = new PaginationVM();
}
