namespace SplitWiseRepository.ViewModels;

public class GroupMemberVM
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int GroupId { get; set; }
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string ProfileImagePath { get; set; }
    public decimal Expense { get; set; } = 0;
}
