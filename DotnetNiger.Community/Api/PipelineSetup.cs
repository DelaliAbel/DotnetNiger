using DotnetNiger.Community.Api.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

namespace DotnetNiger.Community.Api;

/// <summary>Configure le pipeline middleware de l'application Community.</summary>
public static class PipelineSetup
{
    /// <summary>Configure le pipeline HTTP.</summary>
    public static WebApplication ConfigureApp(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseMiddleware<ErrorHandlingMiddleware>();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetNiger Community API v1");
            options.RoutePrefix = "swagger";
        });

        app.UseStaticFiles();
        app.UseCors("AllowFrontendOrigins");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
