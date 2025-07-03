namespace SplitWiseRepository.ViewModels;

public class FriendVM
{
    public int FriendId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string ProfileImagePath { get; set; }
    public decimal Expense { get; set; } = 0;
}
