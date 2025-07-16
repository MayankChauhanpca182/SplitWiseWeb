using SplitWiseRepository.Attributes;

namespace SplitWiseRepository.ViewModels;

public class PaymentVM
{
    [ExcelColumn("Date")]
    public string Date { get; set; } = string.Empty;

    [ExcelColumn("Time")]
    public string Time { get; set; } = string.Empty;
    
    [ExcelColumn("Paid By")]
    public string PaidBy { get; set; } = string.Empty;

    [ExcelColumn("Paid To")]
    public string PaidTo { get; set; } = string.Empty;

    [ExcelColumn("Amount")]
    public string Amount { get; set; } = string.Empty;
}
