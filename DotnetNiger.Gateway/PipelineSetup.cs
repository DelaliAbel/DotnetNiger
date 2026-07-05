using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;

namespace DotnetNiger.Gateway;

/// <summary>Configure le pipeline HTTP de la Gateway.</summary>
public static class PipelineSetup
{
    /// <summary>Configure CORS et les en-têtes forwarded, retourne l'application prête à démarrer.</summary>
    public static WebApplication ConfigureApp(this WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseCors();
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedFor
        });

        return app;
    }
}
