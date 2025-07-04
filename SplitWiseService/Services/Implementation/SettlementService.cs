using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class SettlementService : ISettlementService
{
    private readonly IGenericRepository<Friend> _friendRepository;
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly IGenericRepository<Group> _groupRepository;
    private readonly IGroupService _groupService;
    private readonly IUserService _userService;

    public SettlementService(IGroupService groupService, IUserService userService, IGenericRepository<Friend> friendRepository, IGenericRepository<Expense> expenseRepository, IGenericRepository<Group> groupRepository)
    {
        _groupService = groupService;
        _userService = userService;
        _friendRepository = friendRepository;
        _expenseRepository = expenseRepository;
        _groupRepository = groupRepository;
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
            where e.PaidById == currentUser.Id || es.UserId == currentUser.Id
            group new { e, es } by (int)e.GroupId into g
            select new
            {
                GroupId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUser.Id ? x.es.ShareAmount : -x.es.ShareAmount)
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
        }).Where(g => g.Expense < 0).ToList();

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
            where es.DeletedAt == null && ((e.PaidById == currentUser.Id && es.UserId == friendUserId)
                                        || (es.UserId == currentUser.Id && e.PaidById == friendUserId))
            group new { e, es } by (e.PaidById == currentUser.Id ? es.UserId : e.PaidById) into g
            select new
            {
                FriendUserId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUser.Id ? x.es.ShareAmount : -x.es.ShareAmount)
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
            Expense = netAmount < 0 ? netAmount : 0
        };

        // Set total
        settlementList.TotalAmount = settlementList.Groups.Sum(g => g.Expense) + settlementList.Friend.Expense;

        return settlementList;
    }

    
}
