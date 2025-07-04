using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseRepository.ViewModels;
using SplitWiseService.Constants;
using SplitWiseService.Helpers;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class GroupService : IGroupService
{
    private readonly IGenericRepository<Group> _groupRepository;
    private readonly IGenericRepository<GroupMember> _groupMemberRepository;
    private readonly IGenericRepository<Expense> _expenseRepository;
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;
    private readonly IEmailService _emailService;

    public GroupService(IGenericRepository<Group> groupRepository, ITransactionRepository transaction, IUserService userService, IGenericRepository<GroupMember> groupMemberRepository, IEmailService emailService, IGenericRepository<Expense> expenseRepository)
    {
        _groupRepository = groupRepository;
        _transaction = transaction;
        _userService = userService;
        _groupMemberRepository = groupMemberRepository;
        _emailService = emailService;
        _expenseRepository = expenseRepository;
    }

    private async Task AddMember(int groupId, int userId)
    {
        int currentUserId = _userService.LoggedInUserId();
        GroupMember deletedGroupMember = await _groupMemberRepository.Get(gm => gm.GroupId == groupId && gm.UserId == userId && gm.DeletedAt != null);
        if (deletedGroupMember != null)
        {
            // Update deleted record
            deletedGroupMember.DeletedAt = null;
            deletedGroupMember.DeletedById = null;
            deletedGroupMember.UpdatedAt = DateTime.Now;
            deletedGroupMember.UpdatedById = currentUserId;
            await _groupMemberRepository.Update(deletedGroupMember);
        }
        else
        {
            // Add group member
            GroupMember newGroupMember = new GroupMember
            {
                GroupId = groupId,
                UserId = userId,
                CreatedById = currentUserId,
                UpdatedAt = DateTime.Now,
                UpdatedById = currentUserId
            };
            await _groupMemberRepository.Add(newGroupMember);
        }
        return;
    }

    public async Task<GroupVM> GetGroup(int groupId)
    {
        GroupVM groupVM = new GroupVM();
        Group group = await _groupRepository.Get(g => g.Id == groupId && g.DeletedAt == null);
        if (group != null)
        {
            groupVM.Id = group.Id;
            groupVM.Name = group.Name;
            groupVM.ImagePath = group.ImagePath;
            groupVM.NoticeBoard = group.NoticeBoard;
            groupVM.CurrencyId = group.CurrencyId;
            groupVM.IsSimplifiedPayments = group.IsSimplifiedPayments;
        }
        return groupVM;
    }

    public async Task<ResponseVM> SaveGroup(GroupVM newGroupVm)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            // Current user id
            User currentUser = await _userService.LoggedInUser();

            if (newGroupVm.Id > 0)
            {
                // Fetch group and update it
                Group exisitngGroup = await _groupRepository.Get(g => g.Id == newGroupVm.Id && g.DeletedAt == null);
                if (exisitngGroup == null)
                {
                    response.Success = false;
                    response.Message = NotificationMessages.NotFound.Replace("{0}", "group");
                    return response;
                }
                else
                {
                    exisitngGroup.Name = newGroupVm.Name;
                    exisitngGroup.NoticeBoard = newGroupVm.NoticeBoard;
                    exisitngGroup.CurrencyId = newGroupVm.CurrencyId;
                    exisitngGroup.IsSimplifiedPayments = newGroupVm.IsSimplifiedPayments;
                    if (newGroupVm.Image != null)
                    {
                        exisitngGroup.ImagePath = ImageHelper.UploadImage(newGroupVm.Image, exisitngGroup.ImagePath);
                    }
                    exisitngGroup.UpdatedAt = DateTime.Now;
                    exisitngGroup.UpdatedById = currentUser.Id;
                    await _groupRepository.Update(exisitngGroup);
                }
            }
            else
            {
                // Add new group
                Group newGroup = new Group
                {
                    Name = newGroupVm.Name,
                    NoticeBoard = newGroupVm.NoticeBoard,
                    CurrencyId = newGroupVm.CurrencyId,
                    IsSimplifiedPayments = newGroupVm.IsSimplifiedPayments,
                    CreatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUser.Id
                };
                if (newGroupVm.Image != null)
                {
                    newGroup.ImagePath = ImageHelper.UploadImage(newGroupVm.Image);
                }
                await _groupRepository.Add(newGroup);
                await AddMember(newGroup.Id, currentUser.Id);
            }

            response.Success = true;
            response.Message = NotificationMessages.Saved.Replace("{0}", "Group");

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

    public async Task<PaginatedListVM<GroupVM>> GroupList(FilterVM filter)
    {
        int currentUserId = _userService.LoggedInUserId();
        string searchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(" ", "").ToLower();

        Func<IQueryable<Group>, IOrderedQueryable<Group>> orderBy = q => q.OrderByDescending(g => g.Id);
        if (!string.IsNullOrEmpty(filter.SortColumn))
        {
            switch (filter.SortColumn)
            {
                case "name":
                    orderBy = filter.SortOrder == "asc" ? q => q.OrderBy(g => g.Name) : q => q.OrderByDescending(g => g.Name);
                    break;
                default:
                    break;
            }
        }

        PaginatedItemsVM<Group> paginatedItems = await _groupRepository.PaginatedList(
            predicate: g => g.GroupMembers.Any(gm => gm.UserId == currentUserId && gm.DeletedAt == null) && g.DeletedAt == null
                            && (string.IsNullOrEmpty(searchString) || g.Name.ToLower().Contains(searchString)),
            orderBy: orderBy,
            includes: new List<Expression<Func<Group, object>>>
            {
                g => g.GroupMembers
            },
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        // Group id list
        List<int> groupIds = paginatedItems.Items.Select(g => g.Id).ToList();

        // Calculate net amount
        Dictionary<int, decimal> netAmounts = await (
            from e in _expenseRepository.Query()
            where e.DeletedAt == null && (e.GroupId != null ? groupIds.Contains((int)e.GroupId) : false)
            from es in e.ExpenseShares
            where es.DeletedAt == null && (e.PaidById == currentUserId || es.UserId == currentUserId) && e.PaidById != es.UserId
            group new { e, es } by (int)e.GroupId into g
            select new
            {
                GroupId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUserId ? x.es.ShareAmount : -x.es.ShareAmount)
            }
        ).ToDictionaryAsync(x => x.GroupId, x => x.Expense);

        PaginatedListVM<GroupVM> paginatedList = new PaginatedListVM<GroupVM>();
        paginatedList.List = paginatedItems.Items.Select(g =>
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
        }).ToList();
        paginatedList.Page.SetPagination(paginatedItems.TotalRecords, filter.PageSize, filter.PageNumber);

        return paginatedList;
    }

    public async Task<ResponseVM> DeleteGroup(int groupId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();
            int currentUserId = _userService.LoggedInUserId();

            Group group = await _groupRepository.Get(g => g.Id == groupId && g.DeletedAt == null);
            if (group == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "group");
            }
            else
            {
                // Check for settlement

                // Delete group
                group.DeletedAt = DateTime.Now;
                group.DeletedById = currentUserId;
                await _groupRepository.Update(group);

                // Delete all group members

                response.Success = true;
                response.Message = NotificationMessages.Deleted.Replace("{0}", "Group");
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

    public async Task<List<GroupMemberVM>> GetMembers(int groupId)
    {
        int currentUserId = _userService.LoggedInUserId();

        PaginatedItemsVM<GroupMember> paginatedItems = await _groupMemberRepository.PaginatedList(
            predicate: gm => gm.GroupId == groupId && gm.DeletedAt == null,
            includes: new List<Expression<Func<GroupMember, object>>>
            {
                fr => fr.User
            });

        // Group member id list
        List<int> groupMemberIds = paginatedItems.Items.Where(gm => gm.UserId != currentUserId).Select(gm => gm.UserId).ToList();

        // Calculate net amount
        Dictionary<int, decimal> netAmounts = await (
            from e in _expenseRepository.Query()
            where e.DeletedAt == null && e.GroupId == groupId
            from es in e.ExpenseShares
            where (e.PaidById == currentUserId && groupMemberIds.Contains(es.UserId))
            || (es.UserId == currentUserId && groupMemberIds.Contains(e.PaidById))
            group new { e, es } by (e.PaidById == currentUserId ? es.UserId : e.PaidById) into g
            select new
            {
                UserId = g.Key,
                Expense = g.Sum(x => x.e.PaidById == currentUserId ? x.es.ShareAmount : -x.es.ShareAmount)
            }
        ).ToDictionaryAsync(x => x.UserId, x => x.Expense);

        return paginatedItems.Items.Select(gm =>
        {
            decimal netAmount = netAmounts.ContainsKey(gm.UserId) ? netAmounts[gm.UserId] : 0;

            return new GroupMemberVM
            {
                Id = gm.Id,
                UserId = gm.UserId,
                GroupId = groupId,
                Name = $"{gm.User.FirstName} {gm.User.LastName}",
                EmailAddress = gm.User.EmailAddress,
                ProfileImagePath = gm.User.ProfileImagePath,
                Expense = netAmount
            };
        }).ToList();
    }

    public async Task<ResponseVM> AddGroupMembers(int groupId, int userId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();

            Group group = await _groupRepository.Get(g => g.Id == groupId && g.DeletedAt == null);
            if (group == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "group");
                return response;
            }

            User user = await _userService.GetById(userId);
            if (user == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "user");
                return response;
            }

            User currentUser = await _userService.LoggedInUser();
            await AddMember(groupId, userId);

            // Send email
            await _emailService.AddedToGroupEmail(user.FirstName, $"{currentUser.FirstName} {currentUser.LastName}", group.Name, user.EmailAddress);

            response.Success = true;
            response.Message = NotificationMessages.MemberAddedToGroup.Replace("{0}", $"{user.FirstName} {user.LastName}").Replace("{1}", group.Name);

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

    public async Task<ResponseVM> RemoveGroupMembers(int groupMemberId)
    {
        try
        {
            // Begin transaction
            await _transaction.Begin();
            ResponseVM response = new ResponseVM();
            User currentUser = await _userService.LoggedInUser();

            GroupMember groupMember = await _groupMemberRepository.Get(g => g.Id == groupMemberId && g.DeletedAt == null);
            if (groupMember == null)
            {
                response.Success = false;
                response.Message = NotificationMessages.NotFound.Replace("{0}", "member");
                return response;
            }

            List<int> groupMemberIds = GetMembers(groupMember.GroupId).Result.Select(gm => gm.UserId).ToList();

            // Calculate net amount
            Dictionary<int, decimal> netAmounts = await (
                from e in _expenseRepository.Query()
                where e.DeletedAt == null && e.GroupId == groupMember.GroupId
                from es in e.ExpenseShares
                where (e.PaidById == groupMember.UserId && groupMemberIds.Contains(es.UserId))
                || (es.UserId == groupMember.UserId && groupMemberIds.Contains(e.PaidById))
                group new { e, es } by (e.PaidById == groupMember.UserId ? es.UserId : e.PaidById) into g
                select new
                {
                    UserId = g.Key,
                    Expense = g.Sum(x => x.e.PaidById == groupMember.UserId ? x.es.ShareAmount : -x.es.ShareAmount)
                }
            ).ToDictionaryAsync(x => x.UserId, x => x.Expense);

            if (!netAmounts.All(a => a.Value == 0))
            {
                response.Success = false;
                response.Message = NotificationMessages.SettleBeforeRemove.Replace("{0}", "group member");
            }
            else
            {
                // Delete member
                groupMember.DeletedAt = DateTime.Now;
                groupMember.DeletedById = currentUser.Id;
                await _groupMemberRepository.Update(groupMember);

                User user = await _userService.GetById(groupMember.UserId);
                Group group = await _groupRepository.Get(
                    predicate: g => g.Id == groupMember.GroupId,
                    includes: new List<Expression<Func<Group, object>>>
                    {
                    g => g.GroupMembers
                    }
                    );

                // Delete group if no members
                if (!group.GroupMembers.Any(gm => gm.DeletedAt == null))
                {
                    group.DeletedAt = DateTime.Now;
                    group.DeletedById = currentUser.Id;
                    await _groupRepository.Update(group);
                }

                response.Success = true;
                response.Message = NotificationMessages.MemberRemovedFromGroup.Replace("{0}", $"{user.FirstName} {user.LastName}").Replace("{1}", group.Name);

                if (user != null)
                {
                    if (user.Id == currentUser.Id)
                    {
                        response.Message = NotificationMessages.LeaveGroup.Replace("{0}", group.Name);
                    }
                    else
                    {
                        // Send email
                        await _emailService.RemovedFromGroupEmail(user.FirstName, $"{currentUser.FirstName} {currentUser.LastName}", group.Name, user.EmailAddress);
                    }
                }
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

    public async Task<byte[]> ExportGroups(FilterVM filter)
    {
        PaginatedListVM<GroupVM> paginatedList = await GroupList(filter);
        if (!paginatedList.List.Any())
        {
            return null;
        }
        List<string> columns = new List<string>
        {
            "Name", "Noticeboard", "IsSimplifiedPayments", "Expense"
        };
        return ExcelExportHelper.ExportToExcel(paginatedList.List, columns, "Groups");
    }

}
