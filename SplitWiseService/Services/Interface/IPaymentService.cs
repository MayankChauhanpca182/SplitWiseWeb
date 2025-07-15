using SplitWiseRepository.Models;
using SplitWiseRepository.ViewModels;

namespace SplitWiseService.Services.Interface;

public interface IPaymentService
{
    public Task<PaginatedListVM<Payment>> FriendPaymentList(FilterVM filter, int friendUserId);
    public Task<byte[]> ExportPayments(FilterVM filter, int friendUserId = 0);
}
