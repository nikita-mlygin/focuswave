using Focuswave.FocusSessionService.Application.FocusCycles;
using Focuswave.FocusSessionService.Domain.FocusCycles;
using LanguageExt;
using MongoDB.Driver;
using static LanguageExt.Prelude;

namespace Focuswave.FocusSessionService.Persistence.FocusCycle;

internal class FocusCycleRepository : IFocusCycleRepository
{
    private readonly IMongoCollection<FocusCycleAggregate.Snapshot> collection;

    public FocusCycleRepository(IMongoDatabase db)
    {
        collection = db.GetCollection<FocusCycleAggregate.Snapshot>("focusCycles");

        var indexKeys = Builders<FocusCycleAggregate.Snapshot>.IndexKeys.Ascending(x => x.Userid);
        var indexModel = new CreateIndexModel<FocusCycleAggregate.Snapshot>(
            indexKeys,
            new CreateIndexOptions { Unique = true }
        );
        collection.Indexes.CreateOne(indexModel);
    }

    public async Task<Option<FocusCycleAggregate>> GetByIdAsync(Guid id)
    {
        var doc = await collection.Find(x => x.Id == id).FirstOrDefaultAsync();
        return doc == null ? None : FocusCycleAggregate.Restore(doc);
    }

    public async Task<Option<FocusCycleAggregate>> GetByUserIdAsync(Guid userId)
    {
        var doc = await collection.Find(x => x.Userid == userId).FirstOrDefaultAsync();
        return doc == null ? None : FocusCycleAggregate.Restore(doc);
    }

    public async Task Remove(FocusCycleAggregate focusCycle)
    {
        var filter = Builders<FocusCycleAggregate.Snapshot>.Filter.Eq(
            x => x.Userid,
            focusCycle.UserId
        );
        await collection.DeleteOneAsync(filter);
    }

    public async Task SaveAsync(FocusCycleAggregate focusCycle)
    {
        var snapshot = focusCycle.To();
        var filter = Builders<FocusCycleAggregate.Snapshot>.Filter.Eq(
            x => x.Userid,
            snapshot.Userid
        );
        await collection.ReplaceOneAsync(filter, snapshot, new ReplaceOptions { IsUpsert = true });
    }
}
