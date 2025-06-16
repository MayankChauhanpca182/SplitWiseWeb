using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Models;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ExceptionLogService : IExceptionLogService
{
    private readonly IGenericRepository<ExceptionLog> _exceptionLogRepository;

    public ExceptionLogService(IGenericRepository<ExceptionLog> exceptionLogRepository)
    {
        _exceptionLogRepository = exceptionLogRepository;
    }

    public async Task LogException(Exception exception, HttpContext context)
    {
        // Store exception details in DB
        ExceptionLog exceptionLog = new ExceptionLog
        {
            SessionId = Guid.NewGuid(),
            APIEndPoint = $"{context.Request.Method} {context.Request.Path}",
            ExceptionMessage = exception.Message,
            InnerException = exception.InnerException?.ToString(),
            UserId = GetUserId(context),
            GroupId = GetGroupId(context),
            ExpenseId = GetExpenseId(context),
            MachineName = Environment.MachineName
        };

        await _exceptionLogRepository.Add(exceptionLog);

        return;
    }

    private int GetUserId(HttpContext context)
    {
        Claim? userIdClaim = context.User?.FindFirst("id");
        return int.Parse(userIdClaim.Value);
    }

    private int? GetGroupId(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("groupId", out var groupId))
        {
            return int.TryParse(groupId, out var id) ? id : (int?)null;
        }
        return null;
    }

    private int? GetExpenseId(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("expenseId", out var expenseId))
        {
            return int.TryParse(expenseId, out var id) ? id : (int?)null;
        }
        return null;
    }

}
