using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Assistant.Api.Authentication;
using Assistant.Api.Middleware;
using AssistantDatabase;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Repositories;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.InternalServices;
using AssistantLogic.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase keeps RealizationState as "toDo"/"done"; UserType names are unchanged.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Asystent w codzienności API",
        Version = "v1",
        Description = "Vertical slice: login returns a JWT that later endpoints (/me, plan) will consume."
    });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste the raw accessToken from /api/v1/auth/login. Swagger adds the \"Bearer \" prefix.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(scheme.Reference.Id, scheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
});

// Domain wiring mirrors AsystentView/Program.cs, minus everything session related.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITaskRepository, TaskRepository>();
builder.Services.AddTransient<ITriggerRepository, TriggerRepository>();
builder.Services.AddTransient<IPointRepository, PointRepository>();
builder.Services.AddTransient<IErrorRepository, ErrorRepository>();
builder.Services.AddTransient<ITriggerService, TriggerService>();
builder.Services.AddTransient<IPointService, PointService>();
builder.Services.AddTransient<ITaskService, TaskService>();
builder.Services.AddTransient<IPlanDayService, PlanDayService>();
builder.Services.AddTransient<ICurrentActivityService, CurrentActivityService>();
builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ITimeService, TimeService>();
builder.Services.AddTransient<IWeatherService, WeatherService>();
builder.Services.AddTransient<IErrorService, ErrorService>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();

JwtOptions jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException($"Missing \"{JwtOptions.SectionName}\" configuration section.");

const int minimumKeyBytes = 32;
if (Encoding.UTF8.GetByteCount(jwtOptions.Key) < minimumKeyBytes)
{
    throw new InvalidOperationException(
        $"\"{JwtOptions.SectionName}:Key\" must be at least {minimumKeyBytes} bytes for HS256.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtClaims.UniqueName,
            RoleClaimType = JwtClaims.Role
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();
app.UseMiddleware<ExceptionHandleMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
