using PaySphere.AuthService.Data.Seed;
using PaySphere.AuthService.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogConfiguration(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddSwaggerDocumentation();

builder.Services.AddControllers();

builder.Host.UseSerilog();

var app = builder.Build();

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

app.Run();