using System.Net;
using System.Net.Mail;
using System.Text.Json;
using SplitWiseService.Constants;

namespace SplitWiseWeb.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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
            case SmtpFailedRecipientException:
                code = HttpStatusCode.NotFound;
                message = NotificationMessages.NotFound.Replace("{0}", "Email address");
                break;
            case SmtpException:
                code = HttpStatusCode.BadRequest;
                message = NotificationMessages.EmailSendingFailed;
                break;
            default:
                code = HttpStatusCode.InternalServerError;
                message = NotificationMessages.InternalServerError;
                break;
        }
 
        // Log Exception
        _logger.LogError(exception, NotificationMessages.UnhandledException);

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
