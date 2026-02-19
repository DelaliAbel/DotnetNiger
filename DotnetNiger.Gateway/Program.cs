using Microsoft.OpenApi.Models;
using DotnetNiger.Gateway.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Enregistrer tous les services du Gateway
builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();

// Activer Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "DotnetNiger Gateway - API Documentation";

        // Document du Gateway (toujours disponible)
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API");

        // Document agrégé de tous les services (nécessite que les services soient lancés)
        options.SwaggerEndpoint("/swagger-aggregated/v1/swagger.json", "Tous les services (agrégés)");

        // Services individuels (via reverse proxy)
        options.SwaggerEndpoint("/swagger/identity/v1/swagger.json", "Identity Service");
        options.SwaggerEndpoint("/swagger/community/v1/swagger.json", "Community Service");

        options.RoutePrefix = "swagger";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableValidator();
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}

app.UseHttpsRedirection();

// Activer CORS
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// Routes du reverse proxy
app.MapReverseProxy();

app.Run();
