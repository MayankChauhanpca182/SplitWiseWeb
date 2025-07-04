using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.Extensions.DependencyInjection;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class SettlementService : ISettlementService
{
    private readonly IGenericRepository<Friend> _friendRepository;
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly IGenericRepository<ExpenseShare> _expenseShareRepository;
    private readonly IGenericRepository<Group> _groupRepository;
    private readonly IGenericRepository<Payment> _paymentRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;

    public SettlementService(IUserService userService, IGenericRepository<Friend> friendRepository, IGenericRepository<Expense> expenseRepository, IGenericRepository<Group> groupRepository, ITransactionRepository transaction, IGenericRepository<Payment> paymentRepository, IGenericRepository<ExpenseShare> expenseShareRepository)
    {
        _userService = userService;
        _friendRepository = friendRepository;
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
        _transaction = transaction;
        _paymentRepository = paymentRepository;
        _expenseShareRepository = expenseShareRepository;

    }

    public async Task<SettlementListVM> GetList(int friendUserId)
    {
        User currentUser = await _userService.LoggedInUser();
        SettlementListVM settlementList = new SettlementListVM();
        // Set current user
        settlementList.CurrentUser = currentUser;

        // Fetch groups
        List<Group> groups = await _groupRepository.List(
            predicate: g => g.DeletedAt == null
                            && g.GroupMembers.Any(gm => gm.UserId == currentUser.Id && gm.DeletedAt == null)
                            && g.GroupMembers.Any(gm => gm.UserId == friendUserId && gm.DeletedAt == null),
            includes: new List<Expression<Func<Group, object>>>
            {
                g => g.GroupMembers
            }
        );

        List<int> groupIds = groups.Select(g => g.Id).ToList();

        // Calculate net amount
        Dictionary<int, decimal> netAmounts = await (
            from e in _expenseRepository.Query()
            where e.DeletedAt == null && (e.GroupId != null ? groupIds.Contains((int)e.GroupId) : false)
            from es in e.ExpenseShares
            where e.PaidById == friendUserId && es.UserId == currentUser.Id
            group new { e, es } by (int)e.GroupId into g
            select new
            {
                GroupId = g.Key,
                Expense = g.Sum(x => x.es.ShareAmount)
            }
        ).ToDictionaryAsync(x => x.GroupId, x => x.Expense);

        // Set group list
        settlementList.Groups = groups.Select(g =>
        {
            decimal netAmount = netAmounts.ContainsKey(g.Id) ? netAmounts[g.Id] : 0;

            return new GroupVM
            {
                Id = g.Id,
                Name = g.Name,
                ImagePath = g.ImagePath,
                IsSimplifiedPayments = g.IsSimplifiedPayments,
                NoticeBoard = g.NoticeBoard,
                Expense = netAmount
            };
        }).Where(g => g.Expense > 0).ToList();

        // Fetch friend
        Friend friend = await _friendRepository.Get(
            predicate: f => f.DeletedAt == null
                            && ((f.Friend1 == currentUser.Id && f.Friend2 == friendUserId) || (f.Friend2 == currentUser.Id && f.Friend1 == friendUserId)),
            includes: new List<Expression<Func<Friend, object>>>
            {
                fr => fr.Friend1UserNavigation,
                fr => fr.Friend2UserNavigation
            }
        );

        // Calculate net amount
        decimal netAmount = await (
            from e in _expenseRepository.Query()
            where e.DeletedAt == null && e.GroupId == null
            from es in e.ExpenseShares
            where es.DeletedAt == null && e.PaidById == friendUserId && es.UserId == currentUser.Id
            group new { e, es } by e.PaidById into g
            select new
            {
                FriendUserId = g.Key,
                Expense = g.Sum(x => x.es.ShareAmount)
            }
        ).Select(x => x.Expense).FirstOrDefaultAsync();

        User friendUser = friend.Friend1 == currentUser.Id ? friend.Friend2UserNavigation : friend.Friend1UserNavigation;

        // Set friend expense
        settlementList.Friend = new FriendVM
        {
            FriendId = friend.Id,
            UserId = friendUser.Id,
            Name = $"{friendUser.FirstName} {friendUser.LastName}",
            ProfileImagePath = friendUser.ProfileImagePath,
            Expense = netAmount
        };

        // Set total
        settlementList.TotalAmount = settlementList.Groups.Sum(g => g.Expense) + settlementList.Friend.Expense;

        return settlementList;
    }

    public async Task<ResponseVM> AddSettlement(SettlementVM settlement)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Current user id
            User currentUser = await _userService.LoggedInUser();

            // Record payment
            Payment payment = new Payment
            {
                PaidById = settlement.PaidById,
                PaidToId = settlement.PaidToId,
                CurrencyId = settlement.CurrencyId,
                Amount = settlement.Amount,
                CreatedById = currentUser.Id,
                UpdatedById = currentUser.Id,
                UpdatedAt = DateTime.Now
            };

            if (settlement.Attachment != null)
            {
                payment.AttachmentPath = FileHelper.UploadFile(settlement.Attachment);
                payment.AttachmentName = settlement.Attachment.FileName;
            }
            await _paymentRepository.Add(payment);

            // Update ExpenseShares
            if (settlement.SettleAll)
            {
                // Fetch all expense shares
                List<ExpenseShare> expenseShares = await _expenseShareRepository.List(
                    predicate: es => es.DeletedAt == null && es.UserId == currentUser.Id && es.Expense.PaidById == settlement.PaidToId,
                    includes: new List<Expression<Func<ExpenseShare, object>>>
                    {
                        es => es.Expense
                    }
                );

                foreach (ExpenseShare share in expenseShares)
                {
                    share.ShareAmount = 0;
                    share.UpdatedAt = DateTime.Now;
                    share.UpdatedById = currentUser.Id;
                    await _expenseShareRepository.Update(share);
                }
            }
            else
            {
                decimal remaingAmount = settlement.Amount;

                // Fetch expense share list
                List<ExpenseShare> expenseShares = await _expenseShareRepository.List(
                    predicate: es => es.DeletedAt == null && es.UserId == currentUser.Id && es.Expense.PaidById == settlement.PaidToId
                                && (settlement.GroupId == 0 ? es.Expense.GroupId == null : es.Expense.GroupId == settlement.GroupId),
                    includes: new List<Expression<Func<ExpenseShare, object>>>
                    {
                        es => es.Expense
                    }
                );

                foreach (ExpenseShare share in expenseShares)
                {
                    if (remaingAmount >= share.ShareAmount)
                    {
                        remaingAmount -= share.ShareAmount;
                        share.ShareAmount = 0;
                    }
                    else
                    {
                        share.ShareAmount -= remaingAmount;
                        remaingAmount = 0;
                    }
                    share.UpdatedAt = DateTime.Now;
                    share.UpdatedById = currentUser.Id;
                    await _expenseShareRepository.Update(share);

                    if (remaingAmount == 0)
                    {
                        break;
                    }
                }
            }

            // Send mail

            response.Success = true;
            response.Message = NotificationMessages.SettlementSuccess;

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
