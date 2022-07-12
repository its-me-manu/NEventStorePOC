using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NEventStorePOC.Framework;
using NEventStorePOC.Model;
using NEventStore;

namespace NEventStorePOC.Services;
public class MongoDBService
{
    private readonly IMongoCollection<SampleResponse> _response;
    private readonly IStoreEvents _eventStore;
    public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
    {
        MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
        _response = database.GetCollection<SampleResponse>(mongoDBSettings.Value.CollectionName);

        _eventStore = CreateEventStore(mongoDBSettings.Value.EventStoreDB);

    }

    public async Task CreateAsync(SampleResponse response)
    {
        await _response.InsertOneAsync(response);
        return;
    }

    public async Task<List<SampleResponse>> GetAllAsync()
    {
        return await _response.Find(new BsonDocument()).ToListAsync();
    }

    public async Task DeleteAsync(long id)
    {
        FilterDefinition<SampleResponse> filter = Builders<SampleResponse>.Filter.Eq("id", id);
        await _response.DeleteOneAsync(filter);    
        return;
    }


    private IStoreEvents CreateEventStore(string connectionString)
    {
        var store = Wireup.Init()
                            .UsingMongoPersistence(connectionString, new NEventStore.Serialization.DocumentObjectSerializer())
                            .InitializeStorageEngine()
                            .UsingBsonSerialization()
                            .Compress()
                            .HookIntoPipelineUsing(new DispatchToServiceBusHook())
                            .Build();
        return store;
    }

    public async Task<List<EventStream>> Get(Guid id)
    {
        try
            {
                var events = new List<EventStream>();
                var eventStreams = _eventStore.Advanced.GetFrom("default", id.ToString(), int.MinValue, int.MaxValue);
                foreach (var stream in eventStreams)
                {
                    var data = stream.Events.FirstOrDefault().Body as Command;
                    AddRevisionEventData(events, stream, data.ConvertToJson());
                }
                return events;
            }
            catch (Exception ex)
            {
                throw ex;
            }
    }
    public async Task AddMessageToDB(CommandJSON commandJson)
    {
        try
            {
                using (var stream = _eventStore.CreateStream(commandJson.id))
                {
                    stream.Add(new EventMessage { Body = commandJson.Command });
                    stream.CommitChanges(Guid.NewGuid());
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
    }

    public async Task AppendMessageToDB(CommandJSON commandJson)
    {
        try
            {
                using (var stream = _eventStore.OpenStream(commandJson.id))
                {
                    stream.Add(new EventMessage { Body = commandJson.Command });
                    stream.CommitChanges(Guid.NewGuid());
                    return;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
    }

    public async Task TakeSnapshot(Guid id)
    {
        var payload = "snapshot";
        _eventStore.Advanced.AddSnapshot(new Snapshot(id.ToString(), int.MaxValue, payload));
    }

    public async Task LoadFromSnapshotForwardAndAppend(CommandJSON commandJson)
    {
        var latestSnapshot = _eventStore.Advanced.GetSnapshot(commandJson.id.ToString(), int.MaxValue);

        using (var stream = _eventStore.OpenStream(latestSnapshot, int.MaxValue))
        {
            var @event = "Last event (first one after a snapshot).";

            stream.Add(new EventMessage { Body = @event });
            stream.CommitChanges(Guid.NewGuid());
        }
    }

    private void AddRevisionEventData(List<EventStream> events, ICommit rev, string data)
    {
        events.Add(new EventStream
        {
            EventData = data
        });
    }
}