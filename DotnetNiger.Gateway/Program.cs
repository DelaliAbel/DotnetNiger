using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Ajouter HttpClient
builder.Services.AddHttpClient();

// Ajouter Controllers pour l'agrégateur
builder.Services.AddControllers();

// Ajouter les services pour Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Document pour le Gateway lui-même
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DotnetNiger API Gateway",
        Version = "v1",
        Description = "Gateway d'agrégation pour tous les services microservices DotnetNiger. Ce document affiche les endpoints du Gateway."
    });
});

// Ajouter le reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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

app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();
