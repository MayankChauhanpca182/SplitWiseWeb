namespace SplitWiseRepository.ViewModels;

public class ResponseVM
{
    public long EntityId { get; set; } = 0;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public bool ShowNextAction { get; set; } = false;
}
