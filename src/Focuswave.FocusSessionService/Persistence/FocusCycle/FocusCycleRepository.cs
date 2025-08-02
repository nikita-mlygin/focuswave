using Focuswave.FocusSessionService.Application.FocusCycles;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using LanguageExt;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using static LanguageExt.Prelude;

namespace Focuswave.FocusSessionService.Persistence.FocusCycle;

internal class FocusCycleRepository : IFocusCycleRepository
{
    private readonly IMongoCollection<FocusCycleAggregate.Snapshot> collection;
    private readonly ILogger<FocusCycleRepository> logger;

    public FocusCycleRepository(IMongoDatabase db, ILogger<FocusCycleRepository> logger)
    {
        this.logger = logger;
        collection = db.GetCollection<FocusCycleAggregate.Snapshot>("focusCycles");

        try
        {
            var indexKeys = Builders<FocusCycleAggregate.Snapshot>.IndexKeys.Ascending(x =>
                x.Userid
            );
            var indexModel = new CreateIndexModel<FocusCycleAggregate.Snapshot>(
                indexKeys,
                new CreateIndexOptions { Unique = true }
            );
            collection.Indexes.CreateOne(indexModel);
            logger.LogInformation("Created index on focusCycles collection for Userid");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create index on focusCycles collection");
        }
    }

    public async Task<Option<FocusCycleAggregate>> GetByIdAsync(Guid id)
    {
        logger.LogDebug("Retrieving FocusCycle by Id: {Id}", id);
        var doc = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        if (doc == null)
        {
            logger.LogInformation("FocusCycle not found by Id: {Id}", id);
            return None;
        }

        logger.LogInformation("FocusCycle found by Id: {Id}", id);
        return FocusCycleAggregate.Restore(doc);
    }

    public async Task<Option<FocusCycleAggregate>> GetByUserIdAsync(Guid userId)
    {
        logger.LogDebug("Retrieving FocusCycle by UserId: {UserId}", userId);
        var doc = await collection.Find(x => x.Userid == userId).FirstOrDefaultAsync();
        if (doc == null)
        {
            logger.LogInformation("FocusCycle not found for UserId: {UserId}", userId);
            return None;
        }

        logger.LogInformation("FocusCycle found for UserId: {UserId}", userId);
        return FocusCycleAggregate.Restore(doc);
    }

    public async Task Remove(FocusCycleAggregate focusCycle)
    {
        logger.LogInformation("Removing FocusCycle for UserId: {UserId}", focusCycle.UserId);
        var filter = Builders<FocusCycleAggregate.Snapshot>.Filter.Eq(
            x => x.Userid,
            focusCycle.UserId
        );
        var result = await collection.DeleteOneAsync(filter);

        logger.LogDebug("Delete result: {DeletedCount} document(s)", result.DeletedCount);
    }

    public async Task SaveAsync(FocusCycleAggregate focusCycle)
    {
        logger.LogInformation("Saving FocusCycle for UserId: {UserId}", focusCycle.UserId);
        var snapshot = focusCycle.To();
        var filter = Builders<FocusCycleAggregate.Snapshot>.Filter.Eq(
            x => x.Userid,
            snapshot.Userid
        );
        await collection.ReplaceOneAsync(filter, snapshot, new ReplaceOptions { IsUpsert = true });
        logger.LogDebug("FocusCycle saved (upsert) for UserId: {UserId}", snapshot.Userid);
    }
}
