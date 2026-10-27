using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Assistant.Api.Authentication;
using Assistant.Api.Contracts;
using Assistant.Api.Middleware;
using Assistant.Api.OpenApi;
using Assistant.Api.Storage;
using AssistantDatabase;
using AssistantDatabase.IRepositories;
using AssistantDatabase.Repositories;
using AssistantLogic.IExternalServices;
using AssistantLogic.IInternalServices;
using AssistantLogic.InternalServices;
using AssistantLogic.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    // Keep the default KnownNetworks and KnownProxies so only loopback proxies are trusted.
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddProblemDetails();
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // camelCase keeps RealizationState as "toDo"/"done"; UserType names are unchanged.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Asystent w codzienności API",
        Version = "v1",
        Description = "HTTP JSON API for the Asystent w codzienności KMP clients."
    });

    string xmlDocumentationPath = Path.Combine(
        AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    options.IncludeXmlComments(xmlDocumentationPath, includeControllerXmlComments: true);
    options.SupportNonNullableReferenceTypes();
    options.SchemaFilter<RequiredNotNullableSchemaFilter>();
    options.SchemaFilter<PolymorphismSchemaFilter>();

    options.UseOneOfForPolymorphism();
    options.UseAllOfForInheritance();
    options.SelectSubTypesUsing(baseType =>
    {
        if (baseType == typeof(TriggerDto))
        {
            return new[] { typeof(TimeTriggerDto), typeof(WeatherTriggerDto) };
        }

        if (baseType == typeof(CreateTriggerRequest))
        {
            return new[] { typeof(CreateTimeTriggerRequest), typeof(CreateWeatherTriggerRequest) };
        }

        if (baseType == typeof(UpdateTriggerRequest))
        {
            return new[] { typeof(UpdateTimeTriggerRequest), typeof(UpdateWeatherTriggerRequest) };
        }

        return Array.Empty<Type>();
    });
    options.SelectDiscriminatorNameUsing(_ => "kind");
    options.SelectDiscriminatorValueUsing(subType => subType switch
    {
        Type type when type == typeof(TimeTriggerDto)
            || type == typeof(CreateTimeTriggerRequest)
            || type == typeof(UpdateTimeTriggerRequest) => "time",
        Type type when type == typeof(WeatherTriggerDto)
            || type == typeof(CreateWeatherTriggerRequest)
            || type == typeof(UpdateWeatherTriggerRequest) => "weather",
        _ => subType.Name
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
    options.OperationFilter<PolymorphismOperationFilter>();
    options.OperationFilter<AuthorizeOperationFilter>();
    options.OperationFilter<EmptyResponseOperationFilter>();
});

// Domain wiring mirrors AsystentView/Program.cs, minus everything session related.
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<DataContext>();
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
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtClaims.UniqueName,
            RoleClaimType = JwtClaims.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.HasClaim(
                    JwtClaims.TokenUse,
                    JwtClaims.AccessTokenUse) != true)
                {
                    context.Fail("Only access tokens can authorize API requests.");
                }

                return Task.CompletedTask;
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    new ErrorResponse("Token is missing or invalid") { Code = "unauthorized" },
                    options: null,
                    contentType: "application/json",
                    cancellationToken: context.HttpContext.RequestAborted);
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(
                    new ErrorResponse("You do not have the required role") { Code = "forbidden" },
                    options: null,
                    contentType: "application/json",
                    cancellationToken: context.HttpContext.RequestAborted);
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, cancellationToken) =>
    {
        HttpContext httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
        {
            httpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                .ToString(CultureInfo.InvariantCulture);
        }

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Type = "https://tools.ietf.org/html/rfc6585#section-4",
                Detail = "Too many requests. Please try again later."
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken: cancellationToken);
    };
    options.AddPolicy("login-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
    options.AddPolicy("health-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();
Directory.CreateDirectory(PhotoPaths.GetUploadsRoot(app.Environment));

app.UseForwardedHeaders();
app.UseExceptionHandling();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Deliberately do not enable UseStaticFiles: photos under wwwroot/uploads must only be
// reachable through the authenticated FilesController.
app.MapControllers();

app.MapHealthChecks("/health").RequireRateLimiting("health-policy");

app.Run();

internal sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        IList<object> metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any()
            || !metadata.OfType<IAuthorizeData>().Any())
        {
            return;
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
