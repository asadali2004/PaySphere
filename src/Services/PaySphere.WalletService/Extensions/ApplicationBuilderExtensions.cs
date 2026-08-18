using PaySphere.WalletService.Middleware;

namespace PaySphere.WalletService.Extensions;

/// <summary>
/// Small convenience extension to register middleware in a readable way from Program.cs.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the centralized global exception handling middleware to the pipeline.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
