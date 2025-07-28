using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Engines;
using SixLabors.ImageSharp.Formats.Tiff.Compression.Decompressors;
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
    private readonly IActivityService _activityService;

    public ExpenseService(IGenericRepository<Expense> expenseRepository, ICategoryService categoryService, ICommonService commonService, IGroupService groupService, IUserService userService, IFriendService friendService, ITransactionRepository transaction, IGenericRepository<ExpenseShare> expenseShareRepository, IEmailService emailService, IActivityService activityService)
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
        _activityService = activityService;
    }

    public async Task<ExpenseVM> GetNonGroupExpense(int expenseId = 0)
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

    private async Task UpdateExpenseShare(Expense expense, List<ExpenseShareVM> updatedShares, SplitType splitType, bool isNew, int oldPaidById, decimal amountToBeSettle)
    {
        User currentUser = await _userService.LoggedInUser();

        List<ExpenseShare> existingShares = await _expenseShareRepository.List(es => es.DeletedAt == null && es.ExpenseId == expense.Id);

        HashSet<int> updatedUserIds = updatedShares.Select(es => es.UserId).ToHashSet();

        List<ExpenseShare> sharesToDelete = existingShares.Where(es => !updatedUserIds.Contains(es.UserId)).ToList();
        // Delete shares
        foreach (ExpenseShare share in sharesToDelete)
        {
            share.DeletedAt = DateTime.Now;
            share.DeletedById = currentUser.Id;
            share.UpdatedAt = DateTime.Now;
            share.UpdatedById = currentUser.Id;
            await _expenseShareRepository.Update(share);

            if (share.SettledAmount != 0)
            {
                // Add system generated expense
                Expense systemExpense = new Expense
                {
                    Title = "System Generated",
                    Amount = share.SettledAmount,
                    GroupId = expense.GroupId,
                    PaidById = share.UserId,
                    PaidDate = DateTime.Today,
                    ExpenseCategoryId = expense.ExpenseCategoryId,
                    CurrencyId = expense.CurrencyId,
                    SplitType = SplitType.Equally,
                    IsSystemGenerated = true,
                    CreatedById = currentUser.Id,
                    UpdatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now
                };
                await _expenseRepository.Add(systemExpense);

                // Add expense share
                ExpenseShare systemExpenseShare = new ExpenseShare
                {
                    ExpenseId = systemExpense.Id,
                    UserId = expense.PaidById,
                    ShareAmount = share.SettledAmount,
                    CreatedById = currentUser.Id,
                    UpdatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now
                };
                await _expenseShareRepository.Add(systemExpenseShare);
            }
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
                if (existingShare.UserId == oldPaidById && oldPaidById != expense.PaidById)
                {
                    existingShare.SettledAmount = (-1) * amountToBeSettle;
                }
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

    private async Task<string> GetDifferences(Expense oldExpense, ExpenseVM newExpense)
    {
        string differences = null;

        // Check amount
        decimal newAmount = decimal.Parse(newExpense.Amount.Replace(",", ""));
        if (oldExpense.Amount != newAmount)
        {
            differences += string.IsNullOrEmpty(differences) ? "Updated" : "; Updated";
            differences += $" amount from <strong>₹{oldExpense.Amount:N2}</strong> to <strong>₹{newAmount:N2}</strong>";
        }

        // Check category
        if (oldExpense.ExpenseCategoryId != newExpense.CategoryId)
        {
            differences += string.IsNullOrEmpty(differences) ? "Updated " : "; Updated";

            Category oldCategory = await _categoryService.GetById(oldExpense.ExpenseCategoryId);
            Category newCategory = await _categoryService.GetById(newExpense.CategoryId);

            differences += $" category from <strong>{oldCategory.Name}</strong> to <strong>{newCategory.Name}</strong>";
        }

        // Check PaidBy
            if (oldExpense.PaidById != newExpense.PaidById)
            {
                differences += string.IsNullOrEmpty(differences) ? "Updated " : "; Updated";

                User oldPaidBy = await _userService.GetById(oldExpense.PaidById);
                User newPaidBy = await _userService.GetById(newExpense.PaidById);

                differences += $" paid by from <strong>{oldPaidBy.FirstName + " " + oldPaidBy.LastName}</strong> to <strong>{newPaidBy.FirstName + " " + newPaidBy.LastName}</strong>";
            }

        // Check splittype
        if (oldExpense.SplitType != newExpense.SplitTypeEnum)
        {
            differences += string.IsNullOrEmpty(differences) ? "Updated" : "; Updated";

            string oldSplitType = string.Empty;
            switch (oldExpense.SplitType)
            {
                case SplitType.ByShare:
                    oldSplitType = "by share";
                    break;
                case SplitType.ByPercentage:
                    oldSplitType = "by percentage";
                    break;
                default:
                    oldSplitType = oldExpense.SplitType.ToString().ToLower();
                    break;
            }

            string newSplitType = string.Empty;
            switch (newExpense.SplitTypeEnum)
            {
                case SplitType.ByShare:
                    newSplitType = "by share";
                    break;
                case SplitType.ByPercentage:
                    newSplitType = "by percentage";
                    break;
                default:
                    newSplitType = newExpense.SplitTypeEnum.ToString().ToLower();
                    break;
            }

            differences += $" splittype from <strong>{oldSplitType}</strong> to <strong>{newSplitType}</strong>";
        }

        // Check members
        List<int> oldMembers = oldExpense.ExpenseShares.Where(es => es.DeletedAt == null).Select(es => es.UserId).ToList();
        List<int> newMembers = newExpense.ExpenseShares.Select(es => es.UserId).ToList();

        List<int> removedMembers = oldMembers.Where(m => !newMembers.Contains(m)).ToList();
        List<int> addedMembers = newMembers.Where(m => !oldMembers.Contains(m)).ToList();

        if (removedMembers.Count > 0)
        {
            differences += string.IsNullOrEmpty(differences) ? "Removed" : "; Removed";
            for (int i = 0; i < removedMembers.Count; i++)
            {
                User user = await _userService.GetById(removedMembers[i]);
                differences += $" <strong>{user.FirstName} {user.LastName}</strong>";
                decimal settledAmount = oldExpense.ExpenseShares.Where(es => es.UserId == removedMembers[i]).Select(es => es.SettledAmount).FirstOrDefault();
                if (settledAmount != 0)
                {
                    differences += ":";

                    string direction = string.Empty;
                    string preposition = string.Empty;
                    User payer = await _userService.GetById(newExpense.PaidById);

                    if (settledAmount < 0)
                    {
                        direction = "owe";
                        preposition = "to";
                        settledAmount = (-1) * settledAmount;
                    }
                    else
                    {
                        direction = "are owed";
                        preposition = "from";
                    }

                    differences += $" System has generated an expense of <strong>₹{settledAmount:N2}</strong> - <strong>{user.FirstName + " " + user.LastName}</strong> {direction} <strong>₹{settledAmount:N2}</strong> {preposition} <strong>{payer.FirstName + " " + payer.LastName}</strong>";
                }

                if (i != removedMembers.Count - 1)
                {
                    differences += ",";
                }
            }
        }

        if (addedMembers.Count > 0)
        {
            differences += string.IsNullOrEmpty(differences) ? "Added" : "; Added";
            for (int i = 0; i < addedMembers.Count; i++)
            {
                User user = await _userService.GetById(addedMembers[i]);
                differences += $" <strong>{user.FirstName} {user.LastName}</strong>";
                if (i != addedMembers.Count - 1)
                {
                    differences += ",";
                }
            }
        }

        return differences;
    }

    public async Task<ResponseVM> SaveExpense(ExpenseVM newExpense)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();
            User currentUser = await _userService.LoggedInUser();
            bool isSplitEqually = newExpense.SplitTypeEnum == SplitType.Equally;
            decimal newAmount = decimal.Parse(newExpense.Amount.Replace(",", ""));

            if (newExpense.Id == 0)
            {
                // Add new expense
                Expense expense = new Expense
                {
                    GroupId = newExpense.GroupId,
                    Title = newExpense.Title,
                    Amount = newAmount,
                    PaidById = newExpense.PaidById,
                    PaidDate = newExpense.PaidDate,
                    ExpenseCategoryId = newExpense.CategoryId,
                    CurrencyId = newExpense.CurrencyId,
                    SplitType = isSplitEqually ? newExpense.SplitTypeEnum : SplitType.Unequally,
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
                await UpdateExpenseShare(expense, newExpense.ExpenseShares, newExpense.SplitTypeEnum, isNew: true, 0, 0);

                if (newExpense.GroupId != null)
                {
                    // Add group activity
                    GroupVM group = await _groupService.GetGroup((int)newExpense.GroupId);
                    await _activityService.AddActivity(ActivityType.GroupExpenseAdded, groupId: (int)newExpense.GroupId, expenseId: expense.Id, groupName: group.Name, amount: newAmount.ToString("N2"));
                }
                else
                {
                    // Add activity
                    await _activityService.AddActivity(ActivityType.ExpenseAdded, expenseId: expense.Id, amount: newAmount.ToString("N2"));
                }

                response.Success = true;
                response.Message = NotificationMessages.Saved.Replace("{0}", "Expense");
            }
            else
            {
                // Fetch expense
                Expense existingExpense = await _expenseRepository.Get(
                    predicate: e => e.Id == newExpense.Id,
                    includes: new List<Expression<Func<Expense, object>>>
                    {
                        e => e.ExpenseShares
                    }
                );

                string additionalDetails = await GetDifferences(existingExpense, newExpense);

                existingExpense.Title = newExpense.Title;
                existingExpense.Amount = newAmount;

                int oldPaidById = existingExpense.PaidById;
                decimal amountToBeSettle = existingExpense.ExpenseShares.Where(es => es.DeletedAt == null && es.UserId != oldPaidById).Sum(es => es.SettledAmount);

                existingExpense.PaidById = newExpense.PaidById;
                existingExpense.PaidDate = newExpense.PaidDate;
                existingExpense.ExpenseCategoryId = newExpense.CategoryId;
                existingExpense.CurrencyId = newExpense.CurrencyId;
                existingExpense.SplitType = isSplitEqually ? newExpense.SplitTypeEnum : SplitType.Unequally;
                existingExpense.Note = newExpense.Note;
                existingExpense.UpdatedAt = DateTime.Now;
                existingExpense.UpdatedById = currentUser.Id;

                if (newExpense.Attachment != null)
                {
                    existingExpense.AttachmentName = newExpense.Attachment.FileName;
                    existingExpense.AttachmentPath = FileHelper.UploadFile(newExpense.Attachment, existingExpense.AttachmentPath);
                }

                await _expenseRepository.Update(existingExpense);
                if (existingExpense.GroupId != null)
                {
                    // Add group activity
                    GroupVM group = await _groupService.GetGroup((int)newExpense.GroupId);
                    await _activityService.AddActivity(ActivityType.GroupExpenseUpdated, groupId: (int)existingExpense.GroupId, expenseId: existingExpense.Id, additionalDetails: additionalDetails, groupName: group.Name, amount: newAmount.ToString("N2"));
                }
                else
                {
                    // Add activity
                    await _activityService.AddActivity(ActivityType.ExpenseUpdated, expenseId: existingExpense.Id, additionalDetails: additionalDetails, amount: newAmount.ToString("N2"));
                }

                // Add expense splits
                await UpdateExpenseShare(existingExpense, newExpense.ExpenseShares, newExpense.SplitTypeEnum, isNew: false, oldPaidById, amountToBeSettle);

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

    public async Task<PaginatedListVM<ExpenseVM>> ExpenseList(FilterVM filter, bool isAllExpense = false, int groupId = 0, int friendUserId = 0)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(" ", "").ToLower();
        bool isGroupExpenses = groupId > 0;

        Func<IQueryable<Expense>, IOrderedQueryable<Expense>> orderBy = q => q.OrderByDescending(e => e.PaidDate).ThenByDescending(e => e.UpdatedAt);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "title":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.Title) : q => q.OrderByDescending(e => e.Title);
                    break;
                case "date":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(e => e.PaidDate).ThenBy(e => e.CreatedAt) : q => q.OrderByDescending(e => e.PaidDate).ThenByDescending(e => e.CreatedAt);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Expense> paginatedItems = await _expenseRepository.PaginatedList(
            predicate: e => (e.PaidById == currentUserId || e.ExpenseShares.Any(es => es.DeletedAt == null && es.UserId == currentUserId))
                            && e.DeletedAt == null
                            && (isAllExpense ? true : (isGroupExpenses ? e.GroupId != null : e.GroupId == null))
                            && (groupId == 0 || e.GroupId == groupId)
                            && (friendUserId == 0
                                ? true
                                : (e.PaidById == currentUserId && e.ExpenseShares.Any(es => es.DeletedAt == null && es.UserId == friendUserId))
                                    || (e.PaidById == friendUserId && e.ExpenseShares.Any(es => es.DeletedAt == null && es.UserId == currentUserId))
                                )
                            && (string.IsNullOrEmpty(searchString)
                                || e.Title.ToLower().Contains(searchString)
                                || e.PaidByUser.FirstName.ToLower().Contains(searchString)
                                || e.PaidByUser.LastName.ToLower().Contains(searchString)
                                || (e.PaidByUser.FirstName + e.PaidByUser.LastName).ToLower().Contains(searchString)
                                || (e.GroupId != null && e.Group.Name.ToLower().Contains(searchString))),
            orderBy: orderBy,
            includes: new List<Expression<Func<Expense, object>>>
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
        paginatedList.List = paginatedItems.Items.Select(e =>
        {
            decimal expenseAmount = 0;
            if (friendUserId > 0)
            {
                expenseAmount = e.PaidById == currentUserId
                        ? e.ExpenseShares.Where(es => es.DeletedAt == null && es.UserId == friendUserId).Sum(es => es.ShareAmount - es.SettledAmount)
                        : (-1) * e.ExpenseShares.Where(es => es.DeletedAt == null && es.UserId == currentUserId && e.PaidById == friendUserId).Sum(es => es.ShareAmount - es.SettledAmount);
            }
            else
            {
                expenseAmount = e.PaidById == currentUserId
                        ? e.ExpenseShares.Where(es => es.DeletedAt == null && es.UserId != currentUserId).Sum(es => es.ShareAmount - es.SettledAmount)
                        : (-1) * e.ExpenseShares.Where(es => es.DeletedAt == null && es.UserId == currentUserId).Sum(es => es.ShareAmount - es.SettledAmount);
            }

            return new ExpenseVM
            {
                Id = e.Id,
                GroupId = e.GroupId,
                GroupDetails = e.GroupId != null ? new GroupVM { Name = e.Group.Name } : new GroupVM { Name = "Non-Group" },
                Title = e.Title,
                PaidDate = e.PaidDate,
                PaidById = e.PaidById,
                PaidByName = e.PaidByUser.FirstName + " " + e.PaidByUser.LastName,
                Members = e.ExpenseShares.Where(es => es.DeletedAt == null).Select(es => es.User).ToList(),
                MemberNames = e.ExpenseShares.Where(es => es.DeletedAt == null).Select(es => es.User.FirstName + " " + es.User.LastName).ToList(),
                Amount = e.Amount.ToString("N2"),
                Expense = expenseAmount,
                IsSystemGenerated = e.IsSystemGenerated,
                PaidAmount = e.Amount
            };
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

    public async Task<byte[]> ExportExpenses(FilterVM filter, bool isAllExpense = false, int groupId = 0, int friendUserId = 0)
    {
        filter.PageNumber = 0;
        filter.PageSize = 0;
        PaginatedListVM<ExpenseVM> paginatedList = await ExpenseList(filter, isAllExpense: isAllExpense, groupId: groupId, friendUserId: friendUserId);
        if (!paginatedList.List.Any())
        {
            return null;
        }
        return ExcelExportHelper.ExportToExcel(paginatedList.List.ToList(), filter, "Expenses");
    }
}
