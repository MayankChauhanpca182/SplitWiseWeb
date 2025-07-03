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
    private readonly IEmailService _emailService;

    public ExpenseService(IGenericRepository<Expense> expenseRepository, ICategoryService categoryService, ICommonService commonService, IGroupService groupService, IUserService userService, IFriendService friendService, ITransactionRepository transaction, IGenericRepository<ExpenseShare> expenseShareRepository, IEmailService emailService)
    {
        _expenseRepository = expenseRepository;
        _categoryService = categoryService;
        _commonService = commonService;
        _groupService = groupService;
        _userService = userService;
        _friendService = friendService;
        _transaction = transaction;
        _expenseShareRepository = expenseShareRepository;
        _emailService = emailService;
    }

    public async Task<ExpenseVM> GetIndividualExpense(int expenseId = 0)
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
            expenseVM.AttachmentPath = expense.AttachmentPath;
            expenseVM.AttachmentName = expense.AttachmentName;
            expenseVM.ExpenseShares = expense.ExpenseShares.Where(es => es.DeletedAt == null)
                    .Select(es => new ExpenseShareVM
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        StringAmount = es.ShareAmount.ToString("N2"),
                        UserName = $"{es.User.FirstName} {es.User.LastName}",
                        ProfileImagePath = es.User.ProfileImagePath
                    }).ToList();

            expenseVM.Friends = _friendService.FriendList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
            // Add current user in friendlist
            expenseVM.Friends.Add(new FriendVM
            {
                UserId = currentUser.Id,
                Name = $"{currentUser.FirstName} {currentUser.LastName}",
                ProfileImagePath = currentUser.ProfileImagePath
            });

            // Add expense members into friend list who are not in friend list
            expenseVM.Friends = expenseVM.Friends.Concat(expenseVM.ExpenseShares.Where(es => !expenseVM.Friends.Any(f => f.UserId == es.UserId)).Select(es => new FriendVM
            {
                UserId = es.UserId,
                Name = es.UserName,
                ProfileImagePath = es.ProfileImagePath
            }).ToList()).ToList();

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

            expenseVM.Friends = _friendService.FriendList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
            // Add current user in friendlist
            expenseVM.Friends.Add(new FriendVM
            {
                UserId = currentUser.Id,
                Name = $"{currentUser.FirstName} {currentUser.LastName}",
                ProfileImagePath = currentUser.ProfileImagePath
            });
        }

        expenseVM.Categories = await _categoryService.GetList();
        expenseVM.Currencies = await _commonService.CurrencyList();
        return expenseVM;
    }

    public async Task<ExpenseVM> GetGroupExpense(int expenseId = 0, int groupId = 0)
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
            expenseVM.AttachmentPath = expense.AttachmentPath;
            expenseVM.AttachmentName = expense.AttachmentName;
            expenseVM.ExpenseShares = expense.ExpenseShares.Where(es => es.DeletedAt == null)
                    .Select(es => new ExpenseShareVM
                    {
                        Id = es.Id,
                        UserId = es.UserId,
                        StringAmount = es.ShareAmount.ToString("N2"),
                        UserName = $"{es.User.FirstName} {es.User.LastName}",
                        ProfileImagePath = es.User.ProfileImagePath
                    }).ToList();

            expenseVM.GroupDetails = await _groupService.GetGroup((int)expense.GroupId);
            expenseVM.Friends = _groupService.GetMembers((int)expense.GroupId).Result.Select(gm => new FriendVM
            {
                UserId = gm.UserId,
                Name = gm.Name,
                ProfileImagePath = gm.ProfileImagePath
            }).ToList();
        }
        else if (groupId > 0)
        {
            expenseVM.GroupId = groupId;
            expenseVM.GroupDetails = await _groupService.GetGroup(groupId);
            expenseVM.ExpenseShares = _groupService.GetMembers((int)groupId).Result.Select(gm => new ExpenseShareVM
            {
                UserId = gm.UserId,
                StringAmount = "0.00",
                UserName = gm.Name,
                ProfileImagePath = gm.ProfileImagePath
            }).ToList();

            expenseVM.Friends = _groupService.GetMembers((int)groupId).Result.Select(gm => new FriendVM
            {
                UserId = gm.UserId,
                Name = gm.Name,
                ProfileImagePath = gm.ProfileImagePath
            }).ToList();
        }
        else
        {
            expenseVM.GroupList = _groupService.GroupList(new FilterVM { PageNumber = 0, PageSize = 0 }).Result.List.ToList();
        }
        expenseVM.Categories = await _categoryService.GetList();
        expenseVM.Currencies = await _commonService.CurrencyList();
        return expenseVM;
    }

    private async Task UpdateExpenseShare(Expense expense, List<ExpenseShareVM> updatedShares, SplitType splitType, bool isNew)
    {
        User currentUser = await _userService.LoggedInUser();

        List<ExpenseShare> existingShares = await _expenseShareRepository.List(es => es.ExpenseId == expense.Id);

        HashSet<int> updatedUserIds = updatedShares.Select(es => es.UserId).ToHashSet();

        List<ExpenseShare> sharesToDelete = existingShares.Where(es => !updatedUserIds.Contains(es.UserId)).ToList();
        // Delete shares
        foreach (ExpenseShare share in sharesToDelete)
        {
            share.DeletedAt = DateTime.Now;
            share.DeletedById = currentUser.Id;
            await _expenseShareRepository.Update(share);
        }

        foreach (ExpenseShareVM share in updatedShares)
        {
            decimal shareAmount = 0;
            string splitTypeName;

            switch (splitType)
            {
                case SplitType.ByShare:
                    splitTypeName = "by share";
                    decimal totalShare = updatedShares.Sum(es => es.ShareAmount);
                    shareAmount = expense.Amount * share.ShareAmount / totalShare;
                    break;
                case SplitType.ByPercentage:
                    splitTypeName = "by percentage";
                    shareAmount = expense.Amount * share.ShareAmount / 100;
                    break;
                default:
                    splitTypeName = splitType.ToString().ToLower();
                    shareAmount = share.ShareAmount;
                    break;
            }

            ExpenseShare existingShare = existingShares.FirstOrDefault(es => es.UserId == share.UserId);
            if (existingShare != null)
            {
                existingShare.ShareAmount = shareAmount;
                existingShare.UpdatedAt = DateTime.Now;
                existingShare.UpdatedById = currentUser.Id;
                await _expenseShareRepository.Update(existingShare);
            }
            else
            {
                ExpenseShare newShare = new ExpenseShare
                {
                    ExpenseId = expense.Id,
                    UserId = share.UserId,
                    ShareAmount = shareAmount,
                    CreatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUser.Id
                };
                await _expenseShareRepository.Add(newShare);
            }

            // Send mail to user
            User user = await _userService.GetById(share.UserId);
            GroupVM group = expense.GroupId == null ? new GroupVM() : await _groupService.GetGroup((int)expense.GroupId);
            bool hasUserPaid = user.Id == expense.PaidById;
            string senderName = user.Id == currentUser.Id ? "you" : $"{currentUser.FirstName} {currentUser.LastName}";
            string oweVariable = hasUserPaid ? "are owed" : "owe";
            string shareAmountStr = hasUserPaid ? (expense.Amount - shareAmount).ToString("N2") : shareAmount.ToString("N2");

            if (isNew)
            {
                await _emailService.AddExpense(user.FirstName, senderName, expense.Title, expense.Amount.ToString("N2"), splitTypeName, shareAmountStr, user.EmailAddress, oweVariable, group.Name);
            }
            else
            {
                await _emailService.UpdateExpense(user.FirstName, senderName, expense.Title, expense.Amount.ToString("N2"), splitTypeName, shareAmountStr, user.EmailAddress, oweVariable, group.Name);
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
            User currentUser = await _userService.LoggedInUser();
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
                    CreatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUser.Id
                };

                // If Attachment
                if (newExpense.Attachment != null)
                {
                    expense.AttachmentName = newExpense.Attachment.FileName;
                    expense.AttachmentPath = FileHelper.UploadFile(newExpense.Attachment);
                }
                await _expenseRepository.Add(expense);

                // Add expense splits
                await UpdateExpenseShare(expense, newExpense.ExpenseShares, newExpense.SplitTypeEnum, isNew: true);

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
                existingExpense.UpdatedById = currentUser.Id;

                if (newExpense.Attachment != null)
                {
                    existingExpense.AttachmentName = newExpense.Attachment.FileName;
                    existingExpense.AttachmentPath = FileHelper.UploadFile(newExpense.Attachment, existingExpense.AttachmentPath);
                }

                await _expenseRepository.Update(existingExpense);

                // Add expense splits
                await UpdateExpenseShare(existingExpense, newExpense.ExpenseShares, newExpense.SplitTypeEnum, isNew: false);

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

    public async Task<PaginatedListVM<ExpenseVM>> ExpenseList(FilterVM filter, bool isAllExpense = false, bool isGroupExpenses = false, int groupId = 0)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(" ", "").ToLower();

        Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy = q => q.OrderByDescending(e => e.PaidDate).ThenByDescending(e => e.UpdatedAt);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "title":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.Title) : q => q.OrderByDescending(e => e.Title);
                    break;
                case "date":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.PaidDate).ThenBy(e => e.UpdatedAt) : q => q.OrderByDescending(e => e.PaidDate).ThenByDescending(e => e.UpdatedAt);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Expense> paginatedItems = await _expenseRepository.PaginatedList(
            predicate: e => (e.PaidById == currentUserId || e.ExpenseShares.Any(es => es.UserId == currentUserId))
                            && e.DeletedAt == null
                            && (isAllExpense ? true : (isGroupExpenses ? e.GroupId != null : e.GroupId == null))
                            && (groupId == 0 || e.GroupId == groupId)
                            && (string.IsNullOrEmpty(searchString)
                                || e.Title.ToLower().Contains(searchString)
                                || e.PaidByUser.FirstName.ToLower().Contains(searchString)
                                || e.PaidByUser.LastName.ToLower().Contains(searchString)
                                || (e.PaidByUser.FirstName + e.PaidByUser.LastName).ToLower().Contains(searchString)
                                || (isGroupExpenses && e.Group.Name.ToLower().Contains(searchString))),
            orderBy: orderBy,
            includes: new List<System.Linq.Expressions.Expression<Func<Expense, object>>>
            {
                e => e.ExpenseShares,
                e => e.PaidByUser,
                e => e.Group,
                e => e.PaidByUser
            },
            thenIncludes: new List<Func<IQueryable<Expense>, IQueryable<Expense>>>
            {
                q => q.Include(e => e.ExpenseShares)
                    .ThenInclude(es => es.User)
            },
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        PaginatedListVM<ExpenseVM> paginatedList = new PaginatedListVM<ExpenseVM>();
        paginatedList.List = paginatedItems.Items.Select(e => new ExpenseVM
        {
            Id = e.Id,
            GroupId = e.GroupId,
            GroupDetails = isGroupExpenses ? new GroupVM { Name = e.Group.Name } : new GroupVM(),
            Title = e.Title,
            PaidDate = e.PaidDate,
            PaidById = e.PaidById,
            PaidByName = e.PaidByUser.FirstName + " " + e.PaidByUser.LastName,
            Members = e.ExpenseShares.Where(es => es.DeletedAt == null).Select(es => es.User).ToList(),
            MemberNames = e.ExpenseShares.Where(es => es.DeletedAt == null).Select(es => es.User.FirstName + " " + es.User.LastName).ToList(),
            NetAmount = (e.PaidById == currentUserId ? e.Amount : 0) - e.ExpenseShares.Where(es => es.UserId == currentUserId).Sum(es => es.ShareAmount)
        }).ToList();

        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);
        return paginatedList;
    }

    public async Task<ResponseVM> RemoveAttachment(int expenseId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();
            User currentUser = await _userService.LoggedInUser();

            Expense expense = await _expenseRepository.Get(e => e.Id == expenseId);

            if (expense == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "expense");
            }
            else
            {
                // Delte attachment
                FileHelper.DeleteFile(expense.AttachmentPath);
                expense.AttachmentName = null;
                expense.AttachmentPath = null;
                expense.UpdatedAt = DateTime.Now;
                expense.UpdatedById = currentUser.Id;

                await _expenseRepository.Update(expense);

                response.Success = true;
                response.Message = NotificationMessages.AttachmentRemoved;

                // Commit transaction
                await _transaction.Commit();
            }
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
