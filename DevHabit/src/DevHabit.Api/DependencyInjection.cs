using System.Text;
using Asp.Versioning;
using DevHabit.Api.Database;
using DevHabit.Api.DTOs.Habits;
using DevHabit.Api.Entities;
using DevHabit.Api.Middlewares;
using DevHabit.Api.Services;
using DevHabit.Api.Services.Sorting;
using DevHabit.Api.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Serialization;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace DevHabit.Api;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        // Add Serialization
        // 1.options.ReturnHttpNotAcceptable = true - Returns a 406 Not Acceptable status code if a client requests a format that isn't supported.
        // 2.AddNewtonsoftJson() - Adds Newtonsoft.Json as the JSON serializer(instead of the default System.Text.Json) with camelCase property names(e.g., converting FirstName to firstName in responses).
        // 3.AddXmlSerializerFormatters() - Enables XML formatting support, so clients can request responses in XML format.
        builder.Services.AddControllers(options =>
            {
                options.ReturnHttpNotAcceptable = true;
            })
            .AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver =
                new CamelCasePropertyNamesContractResolver())
            .AddXmlSerializerFormatters();

        // 1.	Gets the first NewtonsoftJsonOutputFormatter from the list of output formatters.
        // 2.	Adds a custom media type CustomMediaTypeNames.Application.HateoasJson (likely something like "application/vnd.hateoas+json") to the supported formats.
        // 3.	This enables HATEOAS (Hypermedia as the Engine of Application State) responses when clients specifically request this format - allowing your API to return JSON with embedded links for navigation.
        builder.Services.Configure<MvcOptions>(options =>
        {
            NewtonsoftJsonOutputFormatter formatter = options.OutputFormatters
                .OfType<NewtonsoftJsonOutputFormatter>()
                .First();

            //formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.Json);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.JsonV2);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJson);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV1);
            formatter.SupportedMediaTypes.Add(CustomMediaTypeNames.Application.HateoasJsonV2);
        });

        // Versioning
        // 1.	options.DefaultApiVersion = new ApiVersion(1.0) - Sets the default API version to 1.0 when no specific version is requested.
        // 2.	options.AssumeDefaultVersionWhenUnspecified = true - When a client doesn't specify a version in their request, the system will use the default version (1.0) rather than returning an error.
        // 3.	options.ReportApiVersions = true - Adds API version information to response headers, helping clients understand which versions are available.
        // 4.	options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options) - When version isn't specified, this selects the current implementation version (rather than always using the minimum supported version).
        // 5.	options.ApiVersionReader = new UrlSegmentApiVersionReader() - Configures the API to read version information from URL path segments (like /api/v1/resource), rather than from query parameters, headers, or media types.
        // 6.	.AddMvc() - Registers MVC-specific components needed for API versioning support.
        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);

                //options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new MediaTypeApiVersionReader("v"),
                    new MediaTypeApiVersionReaderBuilder()
                        .Template("application/vnd.dev-habit.hateoas.{version}+json")
                        .Build());
            })
            .AddMvc();

        builder.Services.AddOpenApi();

        return builder;
    }

    public static WebApplicationBuilder AddErrorHandling(this WebApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        builder.Services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            };
        });

        return builder;
    }

    public static WebApplicationBuilder AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Application))
                .UseSnakeCaseNamingConvention());

        builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
            options.UseNpgsql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Identity))
                .UseSnakeCaseNamingConvention());

        return builder;
    }

    public static WebApplicationBuilder AddObservability(this WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddNpgsql())
            .WithMetrics(metrics => metrics
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation())
            .UseOtlpExporter();

        builder.Logging.AddOpenTelemetry(options =>
        {
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        return builder;
    }

    public static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        builder.Services.AddTransient<SortMappingProvider>();
        builder.Services.AddSingleton<ISortMappingDefinition, SortMappingDefinition<HabitDto, Habit>>(_ =>
            HabitMappings.SortMapping);

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient<DataShapingService>();
        builder.Services.AddTransient<LinkService>();

        builder.Services.AddTransient<TokenProvider>();

        builder.Services.AddMemoryCache();
        builder.Services.AddScoped<UserContext>();

        return builder;
    }

    public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
    {
        // Configure Identity
        builder.Services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

        // Read JWT configuration from appsettings.json
        builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));
        JwtAuthOptions jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>()!;

        // Configure JWT Bearer Authentication
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwtAuthOptions.Issuer,
                    ValidAudience = jwtAuthOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
                };
            });

        // Activate authorization
        builder.Services.AddAuthorization();

        return builder;
    }
}
