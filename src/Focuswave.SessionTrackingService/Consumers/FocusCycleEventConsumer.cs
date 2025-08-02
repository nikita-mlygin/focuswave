using Confluent.Kafka;
using Focuswave.Integration.Events;
using Focuswave.SessionTrackingService.Handlers;

namespace Focuswave.SessionTrackingService.Consumers;

public class FocusCycleEventConsumer : BackgroundService
{
    private readonly ILogger<FocusCycleEventConsumer> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConsumer<string, byte[]> _consumer;
    private const string Topic = "focus-cycle-events";

    public FocusCycleEventConsumer(
        ILogger<FocusCycleEventConsumer> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _scopeFactory = scopeFactory;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "session-tracking-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };

        _consumer = new ConsumerBuilder<string, byte[]>(config).Build();
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(Topic);
        _logger.LogInformation("Subscribed to topic: {Topic}", Topic);

        Thread thread = new(() =>
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var cr = _consumer.Consume(stoppingToken);
                        using var scope = _scopeFactory.CreateScope();
                        var handler =
                            scope.ServiceProvider.GetRequiredService<FocusCycleEventHandler>();

                        handler
                            .HandleAsync(
                                FocusCycleEvent.Parser.ParseFrom(cr.Message.Value),
                                stoppingToken
                            )
                            .GetAwaiter()
                            .GetResult(); // sync context здесь не важен

                        _consumer.Commit(cr);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling Kafka event");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka thread crashed");
            }
            finally
            {
                _consumer.Close();
            }
        })
        {
            IsBackground = true,
        };
        thread.Start();

        return Task.CompletedTask;
    }
}
