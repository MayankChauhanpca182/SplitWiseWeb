using System.Net;
using System.Net.Mail;
using System.Text.Json;
using MailKit.Net.Smtp;
using SplitWiseService.Constants;
using SplitWiseService.Services.Interface;

namespace SplitWiseWeb.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IServiceScopeFactory _scopeFactory;

    public ExceptionHandlingMiddleware(RequestDelegate next, IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _scopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }
 
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code = 0;
        string message;

        switch (exception)
        {
            case SmtpCommandException:
                code = HttpStatusCode.NotFound;
                message = NotificationMessages.InvalidEmailAddress;
                break;
            case SmtpException:
                code = HttpStatusCode.BadRequest;
                message = NotificationMessages.EmailSendingFailed;
                break;
            case FormatException:
                code = HttpStatusCode.BadRequest;
                message = NotificationMessages.Invalid.Replace("{0}", "token");
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                message = NotificationMessages.InternalServerError;
                break;
        }
 
        // Log Exception
        await using AsyncServiceScope scope = _scopeFactory.CreateAsyncScope();
        IExceptionLogService exceptionService = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
        await exceptionService.LogException(exception, context);

        bool isAjaxRequest = context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
 
        if (isAjaxRequest)
        {
            // For AJAX - return JSON response
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;
            context.Response.Headers.Add("X-Error", "true");
 
            var jsonResponse = new
            {
                success = false,
                statusCode = (int)code,
                error = message
            };
            string jsonMessage = JsonSerializer.Serialize(jsonResponse);
            await context.Response.WriteAsync(jsonMessage);
        }
        else
        {
            // For Normal Requests - use TempData for Toastr
            context.Response.Redirect($"/Auth/HandleExceptionWithToaster?message={Uri.EscapeDataString(message)}");
        }
    }
}
