using SplitWiseRepository.Attributes;

namespace SplitWiseRepository.ViewModels;

public class FriendVM
{
    public int FriendId { get; set; }
    public int UserId { get; set; }

    [ExcelColumn("Name")]
    public string Name { get; set; }

    [ExcelColumn("Email")]
    public string EmailAddress { get; set; }
    public string ProfileImagePath { get; set; }
    
    [ExcelColumn("Expense")]
    public decimal Expense { get; set; } = 0;

    public ActivityFilterVM ActivityFilter { get; set; } = new ActivityFilterVM();
}
