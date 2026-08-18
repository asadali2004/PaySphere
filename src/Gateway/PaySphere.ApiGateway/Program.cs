using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace PaySphere.ApiGateway;

// API Gateway using Ocelot to route incoming client requests to downstream services.
// Responsibilities include forwarding Authorization headers, aggregating Swagger
// documentation and providing a basic health endpoint used by orchestration.
public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddJsonFile(
            "ocelot.json",
            optional: false,
            reloadOnChange: true);

        builder.Services.AddOcelot(builder.Configuration);
        builder.Services.AddSwaggerForOcelot(builder.Configuration);

        var app = builder.Build();

        app.UseRouting();

        // Gateway's own endpoints must be handled before Ocelot
        app.Use(async (context, next) =>
        {
            if (context.Request.Path == "/health")
            {
                await context.Response.WriteAsJsonAsync(
                    new { status = "Healthy" });

                return;
            }

            if (context.Request.Path == "/")
            {
                await context.Response.WriteAsync(
                    "PaySphere API Gateway");

                return;
            }

            await next();
        });

        // Aggregated Swagger
        app.UseSwaggerForOcelotUI(options =>
        {
            options.PathToSwaggerGenerator = "/swagger/docs";
        });

        // Ocelot must be last
        await app.UseOcelot();

        app.Run();
    }
}