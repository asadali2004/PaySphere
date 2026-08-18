using PaySphere.AuthService.Middleware;

namespace PaySphere.AuthService.Extensions;

/// <summary>
/// Extension methods for IApplicationBuilder to add custom middleware to the request pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the GlobalExceptionMiddleware to the application's request pipeline.
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(
        this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();

        return app;
    }
}
