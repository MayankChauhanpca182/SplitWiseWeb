namespace SplitWiseRepository.ViewModels;

public class PaginatedListVM<T> where T : class
{
    public IEnumerable<T> List { get; set; } = new List<T>();
    public PaginationVM Page { get; set; } = new PaginationVM();
    public bool IsDeletedData { get; set; } = false;
}
