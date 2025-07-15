using System.Linq.Expressions;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class PaymentService : IPaymentService
{
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly IUserService _userService;

    public PaymentService(IGenericRepository<Payment> paymentRepository, IUserService userService)
    {
        _paymentRepository = paymentRepository;
        _userService = userService;
    }

    public async Task<PaginatedListVM<Payment>> FriendPaymentList(FilterVM filter, int friendUserId)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? string.Empty : filter.SearchString.Replace(" ", "").ToLower();

        Func<IQueryable<Payment>, IOrderedQueryable<Payment>> orderBy = q => q.OrderByDescending(p => p.CreatedAt);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "amount":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(p => p.Amount) : q => q.OrderByDescending(p => p.Amount);
                    break;
                case "date":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(p => p.CreatedAt) : q => q.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Payment> paginatedItems = await _paymentRepository.PaginatedList(
            predicate: p => p.DeletedAt == null
                            && ((p.PaidById == currentUserId && p.PaidToId == friendUserId) || (p.PaidById == friendUserId && p.PaidToId == currentUserId))
                            && (string.IsNullOrEmpty(searchString) || p.Amount.ToString().Contains(searchString)),
            includes: new List<Expression<Func<Payment, object>>>
            {
                p => p.PaidByUser,
                p => p.PaidToUser
            },
            orderBy: orderBy,
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        PaginatedListVM<Payment> paginatedList = new PaginatedListVM<Payment>();
        paginatedList.List = paginatedItems.Items;
        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);

        return paginatedList;
    }

    public async Task<byte[]> ExportPayments(FilterVM filter, int friendUserId = 0)
    {
        filter.PageNumber = 0;
        filter.PageSize = 0;
        PaginatedListVM<Payment> paginatedList = await FriendPaymentList(filter, friendUserId: friendUserId);
        if (!paginatedList.List.Any())
        {
            return null;
        }

        List<PaymentVM> payments = paginatedList.List.Select(p => new PaymentVM
        {
            Date = p.CreatedAt.ToString("dd-MM-yyyy"),
            Time = p.CreatedAt.ToString("HH:mm:ss"),
            PaidBy = p.PaidByUser.FirstName + " " + p.PaidByUser.LastName,
            PaidTo = p.PaidToUser.FirstName + " " + p.PaidToUser.LastName,
            Amount = p.Amount.ToString("N2")
        }).ToList();

        List<string> columns = new List<string>
        {
            "Date", "Time", "PaidBy", "PaidTo", "Amount"
        };
        return ExcelExportHelper.ExportToExcel(payments, columns, "Payments");
    }
}
