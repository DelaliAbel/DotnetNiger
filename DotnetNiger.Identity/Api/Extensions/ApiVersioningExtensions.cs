using Asp.Versioning;

namespace DotnetNiger.Identity.Api.Extensions;

public static class ApiVersioningExtensions
{
    /// <summary>Configure API Versioning (URL segment + default v1.0 + report version dans les réponses).</summary>
    public static IServiceCollection AddApiVersioningWithSwagger(
        this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
