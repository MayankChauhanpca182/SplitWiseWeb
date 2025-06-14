using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SplitWiseRepository.Repositories.Interface;
using SplitWiseService.Services.Interface;

namespace SplitWiseService.Services.Implementation;

public class ExceptionLogService : IExceptionLogService
{
    // private readonly IGenericRepository<>

    public ExceptionLogService()
    {
    }

    public async Task LogException(Exception exception, HttpContext context)
    {
        Guid SessionId = Guid.NewGuid();
        string APIEndPoint = $"{context.Request.Method} {context.Request.Path}";
        string ExceptionMessage = exception.Message;
        string InnerException = exception.InnerException?.ToString();
        int? userId = GetUserId(context);
        int? groupId = GetGroupId(context);
        int? expenseId = GetExpenseId(context);
        DateTime ExceptionAt = DateTime.Now;
        string MachineName = Environment.MachineName;

        // Store exception details in DB

        return;
    }

    private int? GetUserId(HttpContext context)
    {
        Claim? userIdClaim = context.User?.FindFirst("id");
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : (int?)null;
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
