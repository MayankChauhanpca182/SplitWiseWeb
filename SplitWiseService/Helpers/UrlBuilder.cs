using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;

namespace SplitWiseService.Helpers;

public class UrlBuilder
{
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UrlBuilder(IUrlHelperFactory urlHelperFactory, IHttpContextAccessor httpContextAccessor)
    {
        _urlHelperFactory = urlHelperFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public string Create(string action, string controller, string? token = null, string? upTo = null)
    {
        HttpContext httpContext = _httpContextAccessor.HttpContext;
        ActionContext actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        IUrlHelper urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);

        string url = urlHelper.Action(action, controller, new { token, upTo }, httpContext.Request.Scheme)!;

        // Get local IP
        string ip = Dns.GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)?
                    .ToString() ?? "localhost";

        string updatedUrl = url
            .Replace("localhost", ip)
            .Replace("127.0.0.1", ip);

        return updatedUrl;
    }

}
