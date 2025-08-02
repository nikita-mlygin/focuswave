using FastEndpoints;
using Focuswave.Common.Infrastructure.Vault;
using Focuswave.SessionTrackingService.Consumers;
using Focuswave.SessionTrackingService.Handlers;
using Focuswave.SessionTrackingService.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Focuswave.AuthService API", Version = "v1" });
});

builder.Services.AddScoped<FocusCycleEventHandler>();

builder.Services.AddHostedService<FocusCycleEventConsumer>();

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory()) // путь до проекта
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.Development.json", optional: true) // <- добавьте это
    .Build();

var vaultOptions = config.GetSection("Vault").Get<VaultOptions>();

var vaultService =
    new VaultService(vaultOptions ?? throw new ApplicationException("Vault configuration is null"))
    ?? throw new ApplicationException("Cannot create vault service");

var connectionString = vaultService
    .GetSecretAsync("dev/session-tracking-service", "SessionTrackingConnection")
    .Run()
    .AsTask()
    .Result.IfFail(err => throw new ApplicationException($"Ошибка: {err}"))
    .IfNone(() => throw new ApplicationException("Cant get connection string"));

builder.Services.AddDbContext<SessionTrackingDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton(vaultService);

builder.Services.AddFastEndpoints();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Focuswave.AuthService API v1");
    });
}

app.MapGet("/", () => "Hello World!");

app.MapFastEndpoints();

app.UseHttpsRedirection();

app.Run();
