namespace SplitWiseRepository.ViewModels;

public class ResponseVM
{
    public long EntityId { get; set; } = 0;
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string StringValue { get; set; } = "";
}
