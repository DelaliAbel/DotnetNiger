using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DotnetNiger.Identity.Web.Infrastructure;

public class TenantPageFilter : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var tenantIdRoute = context.RouteData.Values["tenantId"]?.ToString();
        var tenantIdClaim = context.HttpContext.User.FindFirst("tenant_id")?.Value;
        var isPlatformAdmin = context.HttpContext.User.IsInRole("Admin") || context.HttpContext.User.IsInRole("SuperAdmin");

        if (!string.IsNullOrEmpty(tenantIdRoute))
        {
            if (isPlatformAdmin)
            {
                await next();
                return;
            }

            if (string.IsNullOrEmpty(tenantIdClaim) ||
                !string.Equals(tenantIdRoute, tenantIdClaim, StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }
}
