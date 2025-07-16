using SplitWiseRepository.Attributes;

namespace SplitWiseRepository.ViewModels;

public class ActivityVM
{
    [ExcelColumn("Date")]
    public string Date { get; set; } = string.Empty;

    [ExcelColumn("Time")]
    public string Time { get; set; } = string.Empty;
    
    [ExcelColumn("Activity")]
    public string ActivityMessage { get; set; } = string.Empty;
}
