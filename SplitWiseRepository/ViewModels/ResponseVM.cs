namespace SplitWiseRepository.ViewModels;

public class ResponseVM
{
    public long EntityId { get; set; } = 0;
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string Token { get; set; } = "";
    public string Name { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public bool ShowNextAction { get; set; } = false;
}
