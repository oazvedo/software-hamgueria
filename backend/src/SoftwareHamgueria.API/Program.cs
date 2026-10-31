using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.Mvc;
using SoftwareHamgueria.API.Errors;
using SoftwareHamgueria.API.Filters;
using SoftwareHamgueria.Application;
using SoftwareHamgueria.Infrastructure;
using SoftwareHamgueria.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddAuthorization();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

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

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

// Exposes the entry point to WebApplicationFactory<Program> in the functional tests (ADR 0016).
public partial class Program;
