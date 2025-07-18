using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Constants;
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
    private readonly IActivityService _activityService;
    private readonly IEmailService _emailService;

    public SettlementService(IUserService userService, IGenericRepository<Friend> friendRepository, IGenericRepository<Expense> expenseRepository, IGenericRepository<Group> groupRepository, ITransactionRepository transaction, IGenericRepository<Payment> paymentRepository, IGenericRepository<ExpenseShare> expenseShareRepository, IActivityService activityService, IEmailService emailService)
    {
        _userService = userService;
        _friendRepository = friendRepository;
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
        _transaction = transaction;
        _paymentRepository = paymentRepository;
        _expenseShareRepository = expenseShareRepository;
        _activityService = activityService;
        _emailService = emailService;
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
            where es.DeletedAt == null
                && es.ShareAmount != es.SettledAmount
                && ((e.PaidById == friendUserId && es.UserId == currentUser.Id)
                    || (e.PaidById == currentUser.Id && es.UserId == friendUserId))
            group new { e, es } by (int)e.GroupId into g
            select new
            {
                GroupId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUser.Id ? -(x.es.ShareAmount - x.es.SettledAmount) : (x.es.ShareAmount - x.es.SettledAmount))
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
        decimal netAmount = await _expenseShareRepository.Sum(
            selector: es => es.Expense.PaidById == currentUser.Id ? (es.ShareAmount - es.SettledAmount) : -(es.ShareAmount - es.SettledAmount),
            predicate: es => es.DeletedAt == null && es.Expense.DeletedAt == null && es.Expense.GroupId == null
                    && es.ShareAmount != es.SettledAmount
                    && ((es.Expense.PaidById == currentUser.Id && es.UserId == friendUserId)
                        || (es.Expense.PaidById == friendUserId && es.UserId == currentUser.Id)),
            includes: new List<Expression<Func<ExpenseShare, object>>>
            {
                es => es.Expense
            }
        );

        User friendUser = friend.Friend1 == currentUser.Id ? friend.Friend2UserNavigation : friend.Friend1UserNavigation;

        // Set friend expense
        settlementList.Friend = new FriendVM
        {
            FriendId = friend.Id,
            UserId = friendUser.Id,
            Name = $"{friendUser.FirstName} {friendUser.LastName}",
            ProfileImagePath = friendUser.ProfileImagePath,
            Expense = netAmount < 0 ? (-1) * netAmount : 0
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

            // Send email
            User paidToUser = await _userService.GetById(payment.PaidToId);
            await _emailService.PaymentRecorded($"{currentUser.FirstName} {currentUser.LastName}", $"{paidToUser.FirstName} {paidToUser.LastName}", settlement.Amount.ToString("N2"), paidToUser.EmailAddress);

            // Add activity
            if (settlement.GroupId > 0)
            {
                await _activityService.AddActivity(ActivityType.GroupPaymenent, groupId: settlement.GroupId, performedOnId: settlement.PaidToId, paymentId: payment.Id);
            }
            else
            {
                await _activityService.AddActivity(ActivityType.NonGroupPaymenent, performedOnId: settlement.PaidToId, paymentId: payment.Id);
            }

            // Update ExpenseShares
            if (settlement.SettleAll)
            {
                // Fetch all expense shares
                List<ExpenseShare> expenseShares = await _expenseShareRepository.List(
                    predicate: es => es.DeletedAt == null
                                && es.ShareAmount != es.SettledAmount
                                && ((es.Expense.PaidById == settlement.PaidToId && es.UserId == currentUser.Id)
                                    || (es.Expense.PaidById == currentUser.Id && es.UserId == settlement.PaidToId)),
                    includes: new List<Expression<Func<ExpenseShare, object>>>
                    {
                        es => es.Expense
                    }
                );

                foreach (ExpenseShare share in expenseShares.Where(es => es.ShareAmount > 0))
                {
                    share.SettledAmount = share.ShareAmount;
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
                    predicate: es => es.DeletedAt == null
                                && es.ShareAmount != es.SettledAmount
                                && ((es.UserId == currentUser.Id && es.Expense.PaidById == settlement.PaidToId)
                                    || (es.UserId == settlement.PaidToId && es.Expense.PaidById == currentUser.Id))
                                && (settlement.GroupId == 0 ? es.Expense.GroupId == null : es.Expense.GroupId == settlement.GroupId),
                    includes: new List<Expression<Func<ExpenseShare, object>>>
                    {
                        es => es.Expense
                    }
                );

                // Expenseshares where current user gets from friend
                List<ExpenseShare> currentUserGets = expenseShares.Where(es => (es.Expense.PaidById == currentUser.Id && es.ShareAmount - es.SettledAmount > 0) || (es.Expense.PaidById == settlement.PaidToId && es.ShareAmount - es.SettledAmount < 0)).ToList();

                remaingAmount += currentUserGets.Sum(es => es.ShareAmount > 0 ? es.ShareAmount : -es.ShareAmount);

                // Settle all get expenses
                foreach (ExpenseShare share in currentUserGets)
                {
                    share.SettledAmount = share.ShareAmount;
                    share.UpdatedAt = DateTime.Now;
                    share.UpdatedById = currentUser.Id;
                    await _expenseShareRepository.Update(share);
                }

                // Expenseshares where current user pays from friend
                List<ExpenseShare> currentUserPays = expenseShares.Where(es => (es.Expense.PaidById == currentUser.Id && es.ShareAmount - es.SettledAmount < 0) || (es.Expense.PaidById == settlement.PaidToId && es.ShareAmount - es.SettledAmount > 0)).ToList();

                foreach (ExpenseShare share in currentUserPays)
                {
                    decimal net = share.ShareAmount - share.SettledAmount;
                    if (remaingAmount >= net)
                    {
                        share.SettledAmount = share.ShareAmount;
                        remaingAmount -= net;
                    }
                    else
                    {
                        share.SettledAmount += remaingAmount;
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
