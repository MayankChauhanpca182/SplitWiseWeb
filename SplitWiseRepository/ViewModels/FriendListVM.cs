namespace SplitWiseRepository.ViewModels;

public class FriendListVM
{
    public List<FriendVM> FriendList { get; set; } = new List<FriendVM>();
    public PaginationVM Page { get; set; } = new PaginationVM();
}
