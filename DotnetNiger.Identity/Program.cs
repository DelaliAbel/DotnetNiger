

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DotnetNigerIdentityContextConnection") ?? throw new InvalidOperationException("Connection string 'DotnetNigerIdentityContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllers();

//-----AjouterPourLaCommunicationExterne--------
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsePolicy",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
}
);
//-----------------------------------------------

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Identity Service API",
        Version = "v1",
        Description = "API pour l'authentification et la gestion des identités"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Service v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Identity Service - API Documentation";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
