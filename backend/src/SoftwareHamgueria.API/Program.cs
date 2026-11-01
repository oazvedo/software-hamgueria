using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SoftwareHamgueria.API.Audit;
using Hangfire;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using SoftwareHamgueria.API.Controllers;
using SoftwareHamgueria.API.Errors;
using SoftwareHamgueria.API.Jobs;
using SoftwareHamgueria.API.Observability;
using SoftwareHamgueria.Application.Audit;
using SoftwareHamgueria.API.Health;
using SoftwareHamgueria.API.Filters;
using SoftwareHamgueria.Application;
using SoftwareHamgueria.Application.Configuration;
using SoftwareHamgueria.Infrastructure;
using SoftwareHamgueria.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Outside Development, refuse to start without strong secrets, before anything touches the database (ADR 0024).
if (!builder.Environment.IsDevelopment())
{
    var problems = RequiredSettings.Check(builder.Configuration);
    if (problems.Count > 0)
        throw new InvalidOperationException("Configuração de produção inválida: " + string.Join(" ", problems));
}

builder.Services.AddControllers(o =>
        // Required fields are checked by the FluentValidation validators, in Portuguese (ADR 0021).
        o.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = InvalidModelStateFactory.Create);

// Every error is ProblemDetails with a Portuguese title and a traceId (ADR 0022).
builder.Services.AddProblemDetails(o => o.CustomizeProblemDetails = ctx =>
{
    var problem = ctx.ProblemDetails;
    if (problem.Type is null || !problem.Type.StartsWith("https://hamgueria.dev/"))
        (problem.Type, problem.Title) = ProblemTypes.ForStatus(problem.Status ?? ctx.HttpContext.Response.StatusCode);
    problem.Extensions["traceId"] = GlobalExceptionHandler.TraceId(ctx.HttpContext);
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Software Hamgueria API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido em POST /api/auth/login"
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
    c.UseInlineDefinitionsForEnums();
});

var jwtSecret = builder.Configuration["Auth:JwtSecret"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Auth:Issuer"],
            ValidAudience = builder.Configuration["Auth:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.AddObservability();
// Behind Caddy (ADR 0023) every request comes from the proxy: take the client IP from X-Forwarded-For (one hop).
// The API is only reachable inside the compose network, so the header cannot be forged from outside.
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.ForwardLimit = 1;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});
// Browser telemetry is public: at most 60 posts per minute per client IP (spec 023).
builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    o.AddPolicy(TelemetryController.Policy, http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = builder.Configuration.GetValue("Telemetry:PermitPerMinute", 60), Window = TimeSpan.FromMinutes(1) }));
});
builder.Services.AddAuthorization();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// API-only adapter: the audit trail's author and screen come from the request (ADR 0027).
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditContext, HttpAuditContext>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Applies any pending EF Core migrations (creates the schema on first run).
    // Schema changes now ship as migrations — no more manual DB wipes.
    db.Database.Migrate();
    DatabaseSeeder.Seed(db);
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Software Hamgueria API v1"));
}

app.UseForwardedHeaders();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// Job queue dashboard (ADR 0029): under /api so Caddy proxies it; admin credentials by HTTP Basic.
app.UseHangfireDashboard("/api/jobs", new DashboardOptions
{
    Authorization = [new AdminBasicAuthFilter(app.Configuration)],
    DashboardTitle = "Hamgueria: jobs",
    AppPath = "/producao",
    DisplayStorageConnectionString = false,
});
// Infrastructure probe for compose and Caddy (ADR 0023): only the database, "Healthy" 200 or "Unhealthy" 503.
// A broker outage must not mark the API unhealthy (spec 005, RN-07).
app.MapHealthChecks("/api/health", new HealthCheckOptions { Predicate = c => c.Tags.Contains(HealthTags.Database) });
// Database + queue + events waiting; "Degraded" (queue down) still answers 200 (spec 005, RN-08).
app.MapHealthChecks("/api/health/details", new HealthCheckOptions { ResponseWriter = HealthResponseWriter.WriteDetails });
app.Run();

// Exposes the entry point to WebApplicationFactory<Program> in the functional tests (ADR 0016).
public partial class Program;
