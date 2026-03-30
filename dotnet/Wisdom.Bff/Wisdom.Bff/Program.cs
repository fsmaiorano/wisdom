using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Wisdom.Bff.Aggregations.Courses;
using Wisdom.Bff.Aggregations.Dashboard;
using Wisdom.Bff.Clients;
using Wisdom.BuildingBlocks.Auth;
using Wisdom.BuildingBlocks.Extensions;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

var internalApiKey = builder.Configuration["InternalAuth:ApiKey"]
    ?? throw new InvalidOperationException("InternalAuth:ApiKey is required.");

// ── Reverse proxy with internal-auth transforms ────────────────────────────
// YARP validates JWT at the BFF edge; backend services never see the raw token.
// For every proxied request:
//   1. Strip the Authorization header (JWT stays at the BFF boundary)
//   2. Add X-Internal-Api-Key so backends know the request is trusted
//   3. Forward X-User-Id / X-User-Email from the authenticated principal
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(ctx =>
    {
        ctx.AddRequestTransform(transform =>
        {
            transform.ProxyRequest.Headers.Remove("Authorization");
            transform.ProxyRequest.Headers.TryAddWithoutValidation(
                InternalAuthHandler.ApiKeyHeader, internalApiKey);

            var user = transform.HttpContext.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.GetUserId()?.ToString();
                var email  = user.GetEmail();

                if (userId is not null)
                    transform.ProxyRequest.Headers.TryAddWithoutValidation(
                        InternalAuthHandler.UserIdHeader, userId);
                if (email is not null)
                    transform.ProxyRequest.Headers.TryAddWithoutValidation(
                        InternalAuthHandler.UserEmailHeader, email);
            }

            return ValueTask.CompletedTask;
        });
    });

// ── CORS ───────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

// ── Authentication (JWT Bearer via Keycloak) ───────────────────────────────
// The BFF is the single JWT validation boundary. Backends use InternalAuth.
var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]!;
var keycloakRealm   = builder.Configuration["Keycloak:Realm"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer   = true,
            ValidIssuer      = $"{keycloakBaseUrl}/realms/{keycloakRealm}",
            NameClaimType    = "sub",
            RoleClaimType    = "realm_access"
        };
    });

builder.Services.AddAuthorization();

// ── HTTP clients for aggregation endpoints ─────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<CourseApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:CourseApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<OrderApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:OrderApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<LearningApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:LearningApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

// Expose typed clients through interfaces so handlers (and tests) depend on abstractions
builder.Services.AddTransient<ICourseApiClient>(sp => sp.GetRequiredService<CourseApiClient>());
builder.Services.AddTransient<IOrderApiClient>(sp => sp.GetRequiredService<OrderApiClient>());
builder.Services.AddTransient<ILearningApiClient>(sp => sp.GetRequiredService<LearningApiClient>());

// ── Aggregation handlers ───────────────────────────────────────────────────
builder.Services.AddTransient<CoursePageHandler>();
builder.Services.AddTransient<DashboardHandler>();

// ── OpenAPI / Swagger ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "Wisdom BFF", Version = "v1" }));
builder.Services.AddOpenApi();

// ── Health checks ──────────────────────────────────────────────────────────
builder.Services.AddHealthChecks();

// ──────────────────────────────────────────────────────────────────────────

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Wisdom BFF v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseCors();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// ── BFF aggregation endpoints ──────────────────────────────────────────────
app.MapCoursePageEndpoints();
app.MapDashboardEndpoints();

// ── YARP reverse proxy — must be last ──────────────────────────────────────
app.MapReverseProxy();

app.Run();

namespace Wisdom.Bff
{
    public partial class BffProgram;
}
