using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetNiger.Community.Api;
using DotnetNiger.Community.Api.Middleware;
using DotnetNiger.Community.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "DotnetNiger Community API",
        Version = "v1",
        Description = "API publique de la communaut\u00e9 DotnetNiger - Posts, Events, Resources, Comments, Profile, Admin"
    });
});

builder.Services.AddProblemDetails();

builder.Services.AddCommunityInfrastructure(builder.Configuration);
builder.Services.AddCommunityAuthentication(builder.Configuration);
builder.Services.AddCommunityServices();
builder.Services.AddCommunityHttpClients(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "DotnetNiger Community API v1");
        options.RoutePrefix = "";
    });
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
