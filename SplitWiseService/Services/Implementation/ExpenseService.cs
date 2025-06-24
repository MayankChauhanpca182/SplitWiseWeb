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
            // Expense expense = await _expenseRepository.Get(
            //     predicate: e => e.Id == expenseId,
            //     includes: new List<Expression<Func<Expense, object>>>
            //     {
            //         e => e.ExpenseShares
            //     },
            //     thenIncludes: new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
            //     {
            //         q => q.Include(e => e.ExpenseShares)
            //             .ThenInclude(es => es.User)
            //     }
            // );
            // expenseVM.Id = expense.Id;
            // expenseVM.GroupId = expense.GroupId;
            // expenseVM.Title = expense.Title;
            // expenseVM.Amount = expense.Amount;
            // expenseVM.CategoryId = expense.ExpenseCategoryId;
            // expenseVM.CurrencyId = expense.CurrencyId;
            // expenseVM.PaidById = expense.PaidById;
            // expenseVM.PaymentDate = expense.PaidDate;
            // expenseVM.SplitType = expense.SplitType;

            // expenseVM.ExpenseShares = expense.ExpenseShares
            //         .Select(es => new ExpenseShareVM
            //         {
            //             Id = es.Id,
            //             UserId = es.UserId,
            //             ShareAmount = es.ShareAmount,
            //             UserName = $"{es.User.FirstName} {es.User.LastName}",
            //             ProfileImagePath = es.User.ProfileImagePath
            //         }).ToList();
        }
        else if (groupId != null && groupId > 0)
        {
            // expenseVM.ExpenseShares = await _groupService.GetMembers((int)groupId);
        }
        else
        {
            expenseVM.Friends = _friendService.FriendList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
        }

        expenseVM.CurrentUser = currentUser;
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
                    Note = newExpense.Note
                };

                // If Attachment
                if (newExpense.Attachment != null)
                {
                    expense.AttachmentPath = ImageHelper.UploadImage(newExpense.Attachment);
                }
                await _expenseRepository.Add(expense);

                // Add expense splits
                await UpdateExpenseShare(expense.Id, newExpense.ExpenseShares, expense.Amount, newExpense.SplitTypeEnum);

                response.Success = true;
                response.Message = NotificationMessages.Saved.Replace("{0}", "Expense");
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
}
