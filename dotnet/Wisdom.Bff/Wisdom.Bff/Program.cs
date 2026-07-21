using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Wisdom.Bff.Aggregations.Courses;
using Wisdom.Bff.Aggregations.Dashboard;
using Wisdom.Bff.Auth;
using Wisdom.Bff.Clients;
using Wisdom.Bff.Endpoints;
using Wisdom.Bff.Middleware;

var builder = WebApplication.CreateBuilder(args);

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

// ── Downstream error translation ───────────────────────────────────────────
// Typed clients throw DownstreamHttpException on any non-success response;
// this relays the exact downstream status/body instead of a generic 500.
builder.Services.AddExceptionHandler<DownstreamExceptionHandler>();
builder.Services.AddProblemDetails();

// ── HTTP clients ────────────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthForwardingHandler>();

// Keycloak HTTP client (used by AuthHandler for the BFF-native login exchange)
builder.Services.AddHttpClient("keycloak");

// Raw byte-range client for the video stream proxy — no AuthForwardingHandler,
// StreamEndpoint builds its own headers since anonymous callers are allowed.
builder.Services.AddHttpClient("stream");

// Auth handler for the BFF-native /bff/auth/login endpoint
builder.Services.AddTransient<AuthHandler>();

builder.Services
    .AddHttpClient<UserApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:UserApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<CourseApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:CourseApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<LessonApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:CourseApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<TagApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:CourseApi"]!))
    .AddHttpMessageHandler<AuthForwardingHandler>();

builder.Services
    .AddHttpClient<MediaApiClient>(c =>
        c.BaseAddress = new Uri(builder.Configuration["BackendServices:MediaApi"]!))
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
builder.Services.AddTransient<IUserApiClient>(sp => sp.GetRequiredService<UserApiClient>());
builder.Services.AddTransient<ICourseApiClient>(sp => sp.GetRequiredService<CourseApiClient>());
builder.Services.AddTransient<ILessonApiClient>(sp => sp.GetRequiredService<LessonApiClient>());
builder.Services.AddTransient<ITagApiClient>(sp => sp.GetRequiredService<TagApiClient>());
builder.Services.AddTransient<IMediaApiClient>(sp => sp.GetRequiredService<MediaApiClient>());
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

// ─────────────────────────────────────────────────────────────────────────

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

app.UseExceptionHandler();

app.UseCors();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

// ── BFF-native endpoints ─────────────────────────────────────────────────
app.MapAuthEndpoints();
app.MapCoursePageEndpoints();
app.MapDashboardEndpoints();

// ── Explicit per-operation proxy endpoints ──────────────────────────────
app.MapUserAuthEndpoints();
app.MapUserEndpoints();
app.MapCourseEndpoints();
app.MapLessonEndpoints();
app.MapTagEndpoints();
app.MapMediaEndpoints();
app.MapOrderEndpoints();
app.MapProgressEndpoints();
app.MapStreamEndpoints();

app.Run();
