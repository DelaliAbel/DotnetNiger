using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;
using Serilog.Events;

// ─── Serilog bootstrap (avant la création du host) ───────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Ocelot", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("Démarrage du DotnetNiger API Gateway...");

// ─── Builder ─────────────────────────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// ─── Services ────────────────────────────────────────────────────────────────
builder.Services.AddOcelot(builder.Configuration)
    .AddCacheManager(x => x.WithDictionaryHandle())
    .AddPolly();

builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// ─── JWT (même clé que Identity, partagée avec Community) ────────────────────
var jwtKey = builder.Configuration["Jwt:Key"];
if (!string.IsNullOrWhiteSpace(jwtKey) && jwtKey.Length >= 32 && !jwtKey.StartsWith("__"))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "DotnetNiger.Identity.Client",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    context.Token = authHeader["Bearer ".Length..].Trim();
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

    Log.Information("JWT Authentication configurée (Issuer={Issuer})",
        builder.Configuration["Jwt:Issuer"] ?? "DotnetNiger.Identity");
}
else
{
    Log.Warning("JWT Key non configurée ou invalide — authentification Ocelot désactivée");
}

// ─── App ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseCors("AllowAll");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // app.Urls.Add("http://localhost:5000");
}

// Garantit un identifiant client pour le rate limiting Ocelot
app.Use(async (context, next) =>
{
    var resolvedClientId = context.Request.Headers["ClientId"].FirstOrDefault()
                           ?? context.Request.Headers["Oc-Client"].FirstOrDefault()
                           ?? context.Connection.RemoteIpAddress?.MapToIPv4().ToString()
                           ?? "unknown-client";

    if (!context.Request.Headers.ContainsKey("ClientId"))
        context.Request.Headers["ClientId"] = resolvedClientId;

    if (!context.Request.Headers.ContainsKey("Oc-Client"))
        context.Request.Headers["Oc-Client"] = resolvedClientId;

    await next.Invoke();
});

// Propagation et journalisation du X-Request-ID
app.Use(async (context, next) =>
{
    var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault()
                    ?? Guid.NewGuid().ToString("N");
    context.Response.Headers["X-Request-ID"] = requestId;
    Log.Information("→ {Method} {Path}", context.Request.Method, context.Request.Path);
    await next.Invoke();
    Log.Information("← {StatusCode}", context.Response.StatusCode);
});

// Middleware de fusion — DOIT être AVANT UseSwaggerForOcelotUI pour court-circuiter MMLib
app.Use(async (context, next) =>
{
    if (!context.Request.Path.Equals("/swagger/docs/v1/all", StringComparison.OrdinalIgnoreCase))
    {
        await next(context);
        return;
    }

    var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
    using var client = factory.CreateClient();

    // Appels directs vers MMLib (évite de re-passer par Ocelot)
    string? identityJson = null, communityJson = null;
    try { identityJson = await client.GetStringAsync("http://localhost:5000/swagger/docs/v1/identity"); } catch { }
    try { communityJson = await client.GetStringAsync("http://localhost:5000/swagger/docs/v1/community"); } catch { }

    if (identityJson == null && communityJson == null)
    {
        context.Response.StatusCode = 503;
        return;
    }

    var merged = identityJson ?? communityJson!;
    if (identityJson != null && communityJson != null)
    {
        var idoc = JsonNode.Parse(identityJson)!.AsObject();
        var cdoc = JsonNode.Parse(communityJson)!.AsObject();

        if (idoc["info"] is JsonObject info)
            info["title"] = "DotnetNiger - All APIs";

        var paths = idoc["paths"]?.AsObject() ?? new JsonObject();
        if (cdoc["paths"] is JsonObject cPaths)
            foreach (var p in cPaths)
                paths[p.Key] = p.Value?.DeepClone();
        idoc["paths"] = paths;

        var iComponents = idoc["components"]?.AsObject() ?? new JsonObject();
        var iSchemas = iComponents["schemas"]?.AsObject() ?? new JsonObject();
        if (cdoc["components"]?["schemas"] is JsonObject cSchemas)
            foreach (var s in cSchemas)
                if (!iSchemas.ContainsKey(s.Key))
                    iSchemas[s.Key] = s.Value?.DeepClone();
        iComponents["schemas"] = iSchemas;
        idoc["components"] = iComponents;
        merged = idoc.ToJsonString();
    }

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(merged);
});

app.UseSwaggerForOcelotUI(
    opt =>
    {
        opt.PathToSwaggerGenerator = "/swagger/docs";
    },
    uiOpt =>
    {
        uiOpt.EnableFilter();
        uiOpt.EnableDeepLinking();
        uiOpt.DisplayRequestDuration();
        uiOpt.EnablePersistAuthorization();
        uiOpt.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });

await app.UseOcelot();
await app.RunAsync();



