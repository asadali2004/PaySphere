using PaySphere.AuthService.Data.Seed;
using PaySphere.AuthService.Extensions;
using Serilog;

// Program initializes the AuthService Web API. It configures logging, JWT
// authentication, EF Core database, repositories, application services and
// Swagger documentation. The host also runs a data seeder to provision initial data.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogConfiguration(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddRepositories()
    .AddApplicationServices()
    .AddSwaggerDocumentation();

// Application services
builder.Services.AddScoped<PaySphere.AuthService.Services.Interfaces.IJwtTokenService, PaySphere.AuthService.Services.JwtTokenService>();
builder.Services.AddScoped<PaySphere.AuthService.Services.Interfaces.IAuthService, PaySphere.AuthService.Services.AuthService>();

builder.Services.AddControllers();

builder.Host.UseSerilog();

var app = builder.Build();

// Seed initial data (roles, admin user) - safe to run at startup in dev/test.
await DataSeeder.SeedAsync(app.Services);

app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Lightweight health endpoint for orchestration and load balancers.
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

app.Run();
