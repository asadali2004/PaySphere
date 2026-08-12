using Microsoft.EntityFrameworkCore;
using PaySphere.WalletService.Data;
using PaySphere.WalletService.Extensions;
using PaySphere.WalletService.Repositories;
using PaySphere.WalletService.Repositories.Interfaces;
using PaySphere.WalletService.Services;
using PaySphere.WalletService.Services.Interfaces;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogConfiguration(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddExternalClients(builder.Configuration)
    .AddSwaggerDocumentation();

builder.Services.AddControllers();

builder.Services.AddDbContext<WalletDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Host.UseSerilog();

var app = builder.Build();

app.UseGlobalExceptionMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();