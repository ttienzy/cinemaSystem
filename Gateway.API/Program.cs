using CacheManager.Core;
using CacheManager.Serialization.Json;
using Cinema.Shared.Extensions;
using Gateway.API.Configuration;
using Ocelot.Cache.CacheManager;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/gateway-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.WithProperty("Service", "Gateway.API")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Use Serilog
builder.Host.UseSerilog();

var gatewayOptions = builder.Configuration
    .GetSection("Gateway")
    .Get<GatewayRuntimeOptions>() ?? new GatewayRuntimeOptions();

var ocelotProfile = ResolveOcelotProfile(builder.Environment, gatewayOptions);
var ocelotFolder = Path.Combine("Ocelot", ocelotProfile);

// ✅ LOG: Print Ocelot configuration being used
Log.Information("========================================");
Log.Information("Gateway Configuration");
Log.Information("========================================");
Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
Log.Information("Ocelot Profile: {Profile}", ocelotProfile);
Log.Information("Ocelot Folder: {Folder}", ocelotFolder);
Log.Information("Service Discovery Enabled: {Enabled}", gatewayOptions.ServiceDiscovery.Enabled);
Log.Information("Redis Cache Enabled: {Enabled}", gatewayOptions.Cache.Redis.Enabled);
Log.Information("CORS Allowed Origins: {Origins}", string.Join(", ", gatewayOptions.Cors.AllowedOrigins));
Log.Information("========================================");

builder.Configuration.AddOcelot(
    ocelotFolder,
    builder.Environment,
    MergeOcelotJson.ToMemory);

builder.Services.Configure<GatewayRuntimeOptions>(builder.Configuration.GetSection("Gateway"));

// Add JWT Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

// Add Ocelot services with caching and Polly
var ocelotBuilder = builder.Services.AddOcelot(builder.Configuration);
ocelotBuilder = ConfigureGatewayCache(ocelotBuilder, gatewayOptions.Cache);
ocelotBuilder.AddPolly();

// Add Health Checks
builder.Services.AddHealthChecks();

// Add CORS for gateway-facing clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("CinemaPolicy", policy =>
    {
        policy.WithOrigins(gatewayOptions.Cors.AllowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// CRITICAL: CORS must be BEFORE Ocelot middleware
app.UseCors("CinemaPolicy");

// Required for SignalR/WebSocket proxying through Ocelot
app.UseWebSockets();

// Add Correlation ID middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-Id"))
    {
        context.Request.Headers["X-Correlation-Id"] = Guid.NewGuid().ToString();
    }

    // Add correlation ID to response headers for debugging
    context.Response.Headers["X-Correlation-Id"] = context.Request.Headers["X-Correlation-Id"];

    await next();
});

// Global exception handler
app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Log.Error(ex, "[Gateway] Unhandled exception");

        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message = "Gateway error occurred",
            traceId = context.Request.Headers["X-Correlation-Id"].ToString(),
            errors = new[] { ex.Message }
        });
    }
});

// Add Health Checks endpoint
app.MapHealthChecks("/health");

// ❌ REMOVED: Gateway should NOT validate JWT - let downstream services handle it
// app.UseAuthentication();
// app.UseAuthorization();

// Use Ocelot middleware
await app.UseOcelot();

app.Run();

static string ResolveOcelotProfile(
    IWebHostEnvironment environment,
    GatewayRuntimeOptions gatewayOptions)
{
    if (!string.IsNullOrWhiteSpace(gatewayOptions.OcelotProfile))
    {
        return gatewayOptions.OcelotProfile;
    }

    return environment.IsEnvironment("Docker") ? "Docker" : "Local";
}

static IOcelotBuilder ConfigureGatewayCache(
    IOcelotBuilder ocelotBuilder,
    GatewayCacheOptions cacheOptions)
{
    var redisOptions = cacheOptions.Redis;

    if (!redisOptions.Enabled || string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
    {
        return ocelotBuilder.AddCacheManager(settings => settings.WithDictionaryHandle());
    }

    return ocelotBuilder.AddCacheManager(settings => settings
        .WithJsonSerializer()
        .WithDictionaryHandle()
        .And
        .WithRedisConfiguration(
            "ocelot-redis",
            redisOptions.ConnectionString,
            redisOptions.Database,
            redisOptions.EnableKeyspaceNotifications)
        .WithMaxRetries(redisOptions.MaxRetries)
        .WithRetryTimeout(redisOptions.RetryTimeoutMs)
        .WithRedisBackplane("ocelot-redis")
        .WithRedisCacheHandle("ocelot-redis", true));
}
