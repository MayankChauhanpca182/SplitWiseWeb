namespace SplitWiseRepository.ViewModels;

public class FilterVM
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SearchString { get; set; } = string.Empty;
    public string SortColumn { get; set; } = string.Empty;
    public string SortOrder { get; set; } = "asc";
    public bool IsDeleted { get; set; } = false;
}
