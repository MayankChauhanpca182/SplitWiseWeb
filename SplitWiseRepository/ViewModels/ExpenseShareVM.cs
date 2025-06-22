namespace SplitWiseRepository.ViewModels;

public class ExpenseShareVM
{
    public int Id { get; set; } = 0;
    public int UserId { get; set; }
    public decimal ShareAmount { get; set; } = 0;
    public string UserName { get; set; }
    public string ProfileImagePath { get; set; }
}
