using Confluent.Kafka;
using FastEndpoints;
using Focuswave.Common;
using Focuswave.Common.Infrastructure.Vault;
using Focuswave.FocusSessionService.Infrastructure;
using Focuswave.FocusSessionService.Persistence;
using Focuswave.Integration.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Focuswave.AuthService API", Version = "v1" });
});

builder.Services.AddFastEndpoints();
builder.Services.AddEventDispatcher();

var vaultOptions = builder.Configuration.GetSection("Vault").Get<VaultOptions>();

var vaultService =
    new VaultService(vaultOptions ?? throw new ApplicationException("Vault configuration is null"))
    ?? throw new ApplicationException("Cannot create vault service");

Console.WriteLine(vaultOptions);

Task.Run(vaultService.TestConnection).GetAwaiter().GetResult();

var connectionInfo = vaultService
    .GetSecretsAsync("dev/focus-session-service", ["MongoConnectionString", "DatabaseName"])
    .Run()
    .AsTask()
    .Result.IfFail(err => throw new ApplicationException($"Ошибка: {err.Exception}"))
    .IfNone(() => throw new ApplicationException("Cant get connection string"));

builder.Services.AddPersistence(connectionInfo[0], connectionInfo[1]);

builder.Services.AddSingleton(sp =>
{
    var config = new ProducerConfig
    {
        BootstrapServers = "localhost:9092",
        // другие настройки
    };
    return new ProducerBuilder<string, FocusCycleEvent>(config)
        .SetValueSerializer(new ProtobufSerializer<FocusCycleEvent>())
        .Build();
});

builder.Services.Scan(scan =>
    scan.FromAssembliesOf(
            typeof(Focuswave.FocusSessionService.Application.IntegrationEvents.IntegrationEventFactory) // TODO
        )
        .AddClasses(classes =>
            classes.AssignableTo(typeof(Focuswave.Common.DomainEvents.IEventHandler<>))
        )
        .AsImplementedInterfaces()
        .WithTransientLifetime()
);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Focuswave.AuthService API v1");
    });
}

app.UseHttpsRedirection();

app.UseFastEndpoints();

app.Run();
