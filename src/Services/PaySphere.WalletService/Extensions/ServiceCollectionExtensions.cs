using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaySphere.WalletService.Configurations;
using PaySphere.WalletService.Services;
using PaySphere.WalletService.Services.Interfaces;
using Serilog;
using System.Text;

namespace PaySphere.WalletService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerilogConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSerilog();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
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

    public static IServiceCollection AddExternalClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var authServiceOptions = configuration.GetSection(AuthServiceOptions.SectionName).Get<AuthServiceOptions>() ?? new AuthServiceOptions();

        services.AddHttpClient<IAuthServiceClient, AuthServiceClient>(client =>
        {
            client.BaseAddress = new Uri(authServiceOptions.BaseUrl);
        });

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "PaySphere Wallet Service API",
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
}