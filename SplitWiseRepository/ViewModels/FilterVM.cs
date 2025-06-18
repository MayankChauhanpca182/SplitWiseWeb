namespace SplitWiseRepository.ViewModels;

public class FilterVM
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SearchString { get; set; } = "";
    public string SortColumn { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
}
