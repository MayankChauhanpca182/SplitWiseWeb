namespace SplitWiseRepository.Repositories.Interface;

public interface ITransactionRepository
{
    public Task Begin();
    public Task Commit();
    public Task Rollback();
}
