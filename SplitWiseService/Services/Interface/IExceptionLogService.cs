using Microsoft.AspNetCore.Http;

namespace SplitWiseService.Services.Interface;

public interface IExceptionLogService
{
    public Task LogException(Exception exception, HttpContext context);

}
