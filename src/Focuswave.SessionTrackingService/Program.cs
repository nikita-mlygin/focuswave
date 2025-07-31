using Focuswave.SessionTrackingService.Consumers;
using Focuswave.SessionTrackingService.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddScoped<FocusCycleEventHandler>();

builder.Services.AddHostedService<FocusCycleEventConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "Hello World!");

app.UseHttpsRedirection();

app.Run();
