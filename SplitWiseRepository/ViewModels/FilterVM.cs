namespace SplitWiseRepository.ViewModels;

public class FilterVM
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SearchQuery { get; set; } = "";
    public string SortColumn { get; set; } = "";
    public string SortOrder { get; set; } = "asc";
}
