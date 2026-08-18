using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaySphere.AuthService.Configurations;
using PaySphere.AuthService.Data;
using PaySphere.AuthService.Repositories;
using PaySphere.AuthService.Repositories.Interfaces;
using Serilog;
using System.Text;
using System.IO;

namespace PaySphere.AuthService.Extensions;

/// <summary>
/// Provides extension methods for IServiceCollection to configure services, authentication,
/// logging, database context, repositories, Swagger documentation, and application-level services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures Serilog logging for the application, reading settings from the provided configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddSerilogConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var logDirectory = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "logs"));

        Directory.CreateDirectory(logDirectory);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .WriteTo.File(
                Path.Combine(logDirectory, "auth-service-.txt"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    /// <summary>
    /// Configures JWT authentication for the application using settings from the provided configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // JWT Options
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.SectionName));

        var jwt = configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwt.Key))
                    };
            });

        services.AddAuthorization();

        return services;
    }

    /// <summary>
    /// Configures the database context for the application using SQL Server
    /// and the connection string from the provided configuration.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<PaySphereAuthDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    /// <summary>
    /// Configures the repositories for the application, registering them with the dependency injection container.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }

    /// <summary>
    /// Configures Swagger documentation for the application, including security definitions for JWT Bearer tokens.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PaySphere Auth Service API",
                Version = "v1"
            });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token **_only_**",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            };

            options.AddSecurityDefinition("Bearer", securityScheme);

            var securityRequirement = new OpenApiSecurityRequirement
            {
                { securityScheme, new string[] { } }
            };

            options.AddSecurityRequirement(securityRequirement);
        });

        return services;
    }

    /// <summary>
    ///     
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Application-level services
        services.AddScoped<PaySphere.AuthService.Services.Interfaces.IJwtTokenService, PaySphere.AuthService.Services.JwtTokenService>();
        services.AddScoped<PaySphere.AuthService.Services.Interfaces.IAuthService, PaySphere.AuthService.Services.AuthService>();

        return services;
    }
}
