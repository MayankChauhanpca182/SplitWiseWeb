using SplitWiseRepository.Constants;

namespace SplitWiseRepository.ViewModels;

public class FilterVM
{
    public int PageNumber { get; set; } = DefaultValues.PageNumber;
    public int PageSize { get; set; } = DefaultValues.PageSize;
    public string SearchString { get; set; } = string.Empty;
    public string SortColumn { get; set; } = string.Empty;
    public string SortOrder { get; set; } = DefaultValues.SortOrder;
    public bool IsDeleted { get; set; } = false;
}
