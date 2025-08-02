using Focuswave.FocusSessionService.Application.FocusCycles;
using Focuswave.FocusSessionService.Persistence.FocusCycle;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Focuswave.FocusSessionService.Persistence;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString,
        string databaseName
    )
    {
        services.AddSingleton<IFocusCycleRepository, FocusCycleRepository>(services =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            var client = new MongoClient(connectionString);
            var db = client.GetDatabase(databaseName);

            return new FocusCycleRepository(
                db,
                services.GetRequiredService<ILogger<FocusCycleRepository>>()
            );
        });

        return services;
    }
}
