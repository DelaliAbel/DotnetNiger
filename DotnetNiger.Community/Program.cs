

using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using DotnetNiger.Community.Infrastructure.Data;
using DotnetNiger.Community.Infrastructure.Data.Seeds;
using DotnetNiger.Community.Infrastructure.Repositories;
using DotnetNiger.Community.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configurer SQLite avec la base partagée
var connectionString = builder.Configuration.GetConnectionString("DotnetNigerDb")
    ?? "Data Source=DotnetNiger.db";
builder.Services.AddDbContext<CommunityDbContext>(options =>
    options.UseSqlite(connectionString)
);

// Enregistrer les repositories
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();
builder.Services.AddScoped<IPartnerRepository, PartnerRepository>();
builder.Services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();

// Enregistrer les services
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IPartnerService, PartnerService>();
builder.Services.AddScoped<ITeamMemberService, TeamMemberService>();


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
        Title = "Community Service API",
        Version = "v1",
        Description = "API pour gérer les fonctionnalités de la communauté"
    });
});

var app = builder.Build();

// Appliquer les migrations automatiquement et seeder les données
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CommunityDbContext>();
    dbContext.Database.Migrate();
    await DatabaseSeeder.SeedDataAsync(dbContext);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Community Service v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "Community Service - API Documentation";
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.EnableDeepLinking();
        options.EnableFilter();
    });
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
