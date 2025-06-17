namespace SplitWiseRepository.ViewModels;

public class PaginationVM
{
    public int PageSize { get; set; } = 5; 
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int TotalRecord { get; set; }
    public int FromRec { get; set; }
    public int ToRec { get; set; }
}
