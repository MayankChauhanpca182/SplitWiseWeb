

using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
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
    private readonly ITransactionRepository _transaction;
    private readonly IUserService _userService;

    public GroupService(IGenericRepository<Group> groupRepository, ITransactionRepository transaction, IUserService userService, IGenericRepository<GroupMember> groupMemberRepository)
    {
        _groupRepository = groupRepository;
        _transaction = transaction;
        _userService = userService;
        _groupMemberRepository = groupMemberRepository;
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
                    if (newGroupVm.ImagePath != null)
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
                    ImagePath = ImageHelper.UploadImage(newGroupVm.Image),
                    CreatedById = currentUser.Id,
                    UpdatedAt = DateTime.Now,
                    UpdatedById = currentUser.Id
                };
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
        filter.SearchString = string.IsNullOrEmpty(filter.SearchString) ? "" : filter.SearchString.Replace(@"\s+", "").ToLower();

        Func<IQueryable<Group>, IOrderedQueryable<Group>> orderBy = q => q.OrderBy(g => g.Id);
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
            predicate: g => g.GroupMembers.Any(gm => gm.UserId == currentUserId && gm.DeletedAt != null) && g.DeletedAt == null,
            orderBy: orderBy,
            includes: new List<Expression<Func<Group, object>>>
            {
                fr => fr.GroupMembers
            },
            pageNumber: filter.PageNumber,
            pageSize: filter.PageSize
        );

        PaginatedListVM<GroupVM> paginatedList = new PaginatedListVM<GroupVM>();
        paginatedList.List = paginatedItems.Items.Select(g => new GroupVM
        {
            Id = g.Id,
            Name = g.Name,
            ImagePath = g.ImagePath
        }).ToList();
        paginatedList.Page.SetPagination(paginatedItems.totalRecords, filter.PageSize, filter.PageNumber);

        return paginatedList;
    }

}
