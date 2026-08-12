using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using DotnetNiger.Api.Options;

namespace DotnetNiger.Api.Extensions;

public static class UploadsEndpoints
{
    public static WebApplication MapUploadsEndpoints(this WebApplication app)
    {
        var uploadsConfigured = app.Services.GetRequiredService<IOptions<UploadOptions>>().Value.Path;

        var uploadsRoot = Path.GetFullPath(
            !string.IsNullOrWhiteSpace(uploadsConfigured)
                ? Path.Combine(app.Environment.ContentRootPath, uploadsConfigured)
                : Path.Combine(app.Environment.ContentRootPath, "wwwroot", "uploads"));

        app.MapGet("/uploads/{**path}", (string path) =>
        {
            var filePath = Path.GetFullPath(Path.Combine(uploadsRoot, path));
            if (!filePath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase) || !File.Exists(filePath))
                return Results.NotFound();
            return Results.File(filePath);
        });

        return app;
    }
}