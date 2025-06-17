namespace SplitWiseRepository.ViewModels;

public class PaginatedItemsVM<T> where T : class
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int totalRecords { get; set; }
}
