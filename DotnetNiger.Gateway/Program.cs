using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MMLib.SwaggerForOcelot.DependencyInjection;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Serilog;
using Serilog.Events;


// Configuration Serilog pour le Gateway
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Ocelot", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/gateway-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

Log.Information("🚀 Démarrage du DotnetNiger API Gateway...");

var builder = WebApplication.CreateBuilder(args);

// Configuration Serilog
builder.Host.UseSerilog();

    // Configuration Ocelot
    builder.Configuration
        .SetBasePath(builder.Environment.ContentRootPath)
        .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

    // Add services with Cache Manager
    builder.Services.AddOcelot(builder.Configuration)
        .AddCacheManager(x =>
        {
            x.WithDictionaryHandle();
        });

    // Add API Explorer (required for Swagger)
    builder.Services.AddEndpointsApiExplorer();


// Swagger generator (OBLIGATOIRE)
builder.Services.AddSwaggerGen();

    // Swagger for Ocelot
    builder.Services.AddSwaggerForOcelot(builder.Configuration);

    // CORS Configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader());
    });

    // Configuration JWT pour la validation des tokens
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
                    if (context.Request.Headers.ContainsKey("Authorization"))
                    {
                        var authHeader = context.Request.Headers["Authorization"].ToString();
                        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer ".Length).Trim();
                        }
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    Log.Warning("JWT Authentication failed: {Error}", context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });
        Log.Information("✅ JWT Authentication configurée");
    }
    else
    {
        Log.Warning("⚠️ JWT Key non configurée ou invalide - l'authentification est désactivée");
    }

    var app = builder.Build();

    // Middleware Pipeline
    app.UseCors("AllowAll");

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.Urls.Add("http://localhost:5000");
    }

    // Request/Response Logging Middleware (simple)
    app.Use(async (context, next) =>
    {
        var requestId = context.Request.Headers["X-Request-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString("N");
        context.Response.Headers["X-Request-ID"] = requestId;

        Log.Information("➡️ {Method} {Path}", context.Request.Method, context.Request.Path);

        await next.Invoke();

        Log.Information("⬅️ {StatusCode}", context.Response.StatusCode);
    });

    // Swagger for Ocelot UI
    app.UseSwaggerForOcelotUI(options =>
    {
        options.PathToSwaggerGenerator = "/swagger/docs";
    });

    // Ocelot Middleware - gère toutes les routes depuis ocelot.json
    await app.UseOcelot();

    await app.RunAsync();


