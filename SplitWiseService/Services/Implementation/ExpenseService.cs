using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using SplitWiseRepository.Constants;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ExpenseService : IExpenseService
{
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly IGenericRepository<ExpenseShare> _expenseShareRepository;
    private readonly ITransactionRepository _transaction;
    private readonly ICategoryService _categoryService;
    private readonly ICommonService _commonService;
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;
    private readonly IFriendService _friendService;

    public ExpenseService(IGenericRepository<Expense> expenseRepository, ICategoryService categoryService, ICommonService commonService, IGroupService groupService, IUserService userService, IFriendService friendService, ITransactionRepository transaction, IGenericRepository<ExpenseShare> expenseShareRepository)
    {
        _expenseRepository = expenseRepository;
        _categoryService = categoryService;
        _commonService = commonService;
        _groupService = groupService;
        _userService = userService;
        _friendService = friendService;
        _transaction = transaction;
        _expenseShareRepository = expenseShareRepository;
    }

    public async Task<ExpenseVM> GetExpense(int expenseId = 0, int groupId = 0)
    {
        ExpenseVM expenseVM = new ExpenseVM();
        User currentUser = await _userService.LoggedInUser();

        if (expenseId > 0)
        {
            Expense expense = await _expenseRepository.Get(
                predicate: e => e.Id == expenseId,
                includes: new List<Expression<Func<Expense, object>>>
                {
                    e => e.ExpenseShares
                },
                thenIncludes: new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
                {
                    q => q.Include(e => e.ExpenseShares)
                        .ThenInclude(es => es.User)
                }
            );
            expenseVM.Id = expense.Id;
            expenseVM.GroupId = expense.GroupId;
            expenseVM.Title = expense.Title;
            expenseVM.Amount = expense.Amount.ToString("N2");
            expenseVM.CategoryId = expense.ExpenseCategoryId;
            expenseVM.CurrencyId = expense.CurrencyId;
            expenseVM.PaidById = expense.PaidById;
            expenseVM.PaidDate = expense.PaidDate;
            expenseVM.SplitTypeEnum = expense.SplitType;

            expenseVM.ExpenseShares = expense.ExpenseShares
                    .Where(es => es.DeletedAt == null)
                    .Select(es => new ExpenseShareVM
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        StringAmount = es.ShareAmount.ToString("N2"),
                        UserName = $"{es.User.FirstName} {es.User.LastName}",
                        ProfileImagePath = es.User.ProfileImagePath
                    }).ToList();
        }
        else if (groupId > 0)
        {
            // expenseVM.ExpenseShares = await _groupService.GetMembers((int)groupId);
        }
        else
        {
            // Add current user to expenseshares
            expenseVM.ExpenseShares.Add(new ExpenseShareVM
            {
                UserId = currentUser.Id,
                UserName = $"{currentUser.FirstName} {currentUser.LastName}",
                ProfileImagePath = currentUser.ProfileImagePath
            });
        }

        expenseVM.Friends = _friendService.FriendList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
        expenseVM.Friends.Add(new FriendVM
        {
            UserId = currentUser.Id,
            Name = $"{currentUser.FirstName} {currentUser.LastName}",
            ProfileImagePath = currentUser.ProfileImagePath
        });
        expenseVM.Categories = await _categoryService.GetList();
        expenseVM.Currencies = await _commonService.CurrencyList();
        return expenseVM;
    }

    private async Task UpdateExpenseShare(int expenseId, List<ExpenseShareVM> updatedShares, decimal totalAmount, SplitType splitType)
    {
        int currentUserId = _userService.LoggedInUserId();

        List<ExpenseShare> existingShares = await _expenseShareRepository.List(es => es.ExpenseId == expenseId);

        HashSet<int> updatedUserIds = updatedShares.Select(es => es.UserId).ToHashSet();

        List<ExpenseShare> sharesToDelete = existingShares.Where(es => !updatedUserIds.Contains(es.UserId)).ToList();
        // Delete shares
        foreach (ExpenseShare share in sharesToDelete)
        {
            share.DeletedAt = DateTime.Now;
            share.DeletedById = currentUserId;
            await _expenseShareRepository.Update(share);
        }

        foreach (ExpenseShareVM share in updatedShares)
        {
            decimal shareAmount = 0;

            switch (splitType)
            {
                case SplitType.ByShare:
                    decimal totalShare = updatedShares.Sum(es => es.ShareAmount);
                    shareAmount = (totalAmount * share.ShareAmount) / totalShare;
                    break;
                case SplitType.ByPercentage:
                    shareAmount = (totalAmount * share.ShareAmount) / 100;
                    break;
                default:
                    shareAmount = share.ShareAmount;
                    break;
            }

            ExpenseShare existingShare = existingShares.FirstOrDefault(es => es.UserId == share.UserId);
            if (existingShare != null)
            {
                existingShare.ShareAmount = shareAmount;
                existingShare.UpdatedAt = DateTime.Now;
                existingShare.UpdatedById = currentUserId;
                await _expenseShareRepository.Update(existingShare);
            }
            else
            {
                ExpenseShare newShare = new ExpenseShare
                {
                    ExpenseId = expenseId,
                    UserId = share.UserId,
                    ShareAmount = shareAmount,
                    CreatedById = currentUserId,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUserId
                };
                await _expenseShareRepository.Add(newShare);
            }
        }
        return;
    }

    public async Task<ResponseVM> SaveExpense(ExpenseVM newExpense)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();
            int currentUserId = _userService.LoggedInUserId();
            bool isSplitEqually = newExpense.SplitTypeEnum == SplitWiseRepository.Constants.SplitType.Equally;

            if (newExpense.Id == 0)
            {
                // Add new expense
                Expense expense = new Expense
                {
                    GroupId = newExpense.GroupId,
                    Title = newExpense.Title,
                    Amount = decimal.Parse(newExpense.Amount.Replace(",", "")),
                    PaidById = newExpense.PaidById,
                    PaidDate = newExpense.PaidDate,
                    ExpenseCategoryId = newExpense.CategoryId,
                    CurrencyId = newExpense.CurrencyId,
                    SplitType = isSplitEqually ? newExpense.SplitTypeEnum : SplitWiseRepository.Constants.SplitType.Unequally,
                    Note = newExpense.Note,
                    CreatedById = currentUserId,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUserId
                };

                // If Attachment
                if (newExpense.Attachment != null)
                {
                    expense.AttachmentPath = FileHelper.UploadFile(newExpense.Attachment);
                }
                await _expenseRepository.Add(expense);

                // Add expense splits
                await UpdateExpenseShare(expense.Id, newExpense.ExpenseShares, expense.Amount, newExpense.SplitTypeEnum);

                response.Success = true;
                response.Message = NotificationMessages.Saved.Replace("{0}", "Expense");
            }
            else
            {
                // Update expense
                Expense existingExpense = await _expenseRepository.Get(e => e.Id == newExpense.Id);
                existingExpense.Title = newExpense.Title;
                existingExpense.Amount = decimal.Parse(newExpense.Amount.Replace(",", ""));
                existingExpense.PaidById = newExpense.PaidById;
                existingExpense.PaidDate = newExpense.PaidDate;
                existingExpense.ExpenseCategoryId = newExpense.CategoryId;
                existingExpense.CurrencyId = newExpense.CurrencyId;
                existingExpense.SplitType = isSplitEqually ? newExpense.SplitTypeEnum : SplitWiseRepository.Constants.SplitType.Unequally;
                existingExpense.Note = newExpense.Note;
                existingExpense.UpdatedAt = DateTime.Now;
                existingExpense.UpdatedById = currentUserId;

                if (newExpense.Attachment != null)
                {
                    existingExpense.AttachmentPath = FileHelper.UploadFile(newExpense.Attachment, existingExpense.AttachmentPath);
                }

                await _expenseRepository.Update(existingExpense);

                // Add expense splits
                await UpdateExpenseShare(existingExpense.Id, newExpense.ExpenseShares, existingExpense.Amount, newExpense.SplitTypeEnum);

                response.Success = true;
                response.Message = NotificationMessages.Updated.Replace("{0}", "Expense");
            }

            // Commit transaction
            await _transaction.Commit();
            return response;
        }
        catch
        {
            // Rollback transaction
            await _transaction.Rollback();
            throw;
        }
    }

    public async Task<PaginatedListVM<ExpenseVM>> IndividualList(FilterVM filter)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(@"\s+", "").ToLower();

        Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy = q => q.OrderBy(e => e.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "title":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.Title) : q => q.OrderByDescending(e => e.Title);
                    break;
                case "date":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.PaidDate) : q => q.OrderByDescending(e => e.PaidDate);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Expense> paginatedItems = await _expenseRepository.PaginatedList(
            predicate: e => (e.PaidById == currentUserId || e.ExpenseShares.Any(es => es.UserId == currentUserId))
                            && e.DeletedAt == null
                            && (string.IsNullOrEmpty(searchString) || e.Title.ToLower().Contains(searchString)),
            orderBy: orderBy,
            includes: new List<System.Linq.Expressions.Expression<Func<Expense, object>>>
            {
                e => e.ExpenseShares
            },
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        PaginatedListVM<ExpenseVM> paginatedList = new PaginatedListVM<ExpenseVM>();
        paginatedList.List = paginatedItems.Items.Select(e => new ExpenseVM
        {
            Id = e.Id,
            Title = e.Title,
            PaidDate = e.PaidDate,
            NetAmount = (e.PaidById == currentUserId ? e.Amount : 0) - e.ExpenseShares.Where(es => es.UserId == currentUserId).Sum(es => es.ShareAmount)
        }).ToList();

        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);
        return paginatedList;
    }
}
