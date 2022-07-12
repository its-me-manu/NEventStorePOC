using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NEventStorePOC.Framework;
using NEventStorePOC.Model;
using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;

namespace NEventStorePOC.Services;
public class SQLDBService
{
    private readonly IStoreEvents _eventStore;
    public SQLDBService(IOptions<SQLDBSettings> SQLDBSettings)
    {
        _eventStore = CreateEventStore(SQLDBSettings.Value.ConnectionString);

    }

    private IStoreEvents CreateEventStore(string connectionString)
    {
        DbProviderFactories.RegisterFactory("System.Data.SqlClient", System.Data.SqlClient.SqlClientFactory.Instance);
        var store = Wireup.Init()
                            .UsingSqlPersistence(DbProviderFactories.GetFactory("System.Data.SqlClient"), connectionString) // Connection string is in app.config
                            .WithDialect(new MsSqlDialect())
                            .InitializeStorageEngine()
                            .UsingCustomSerialization(new JsonSerializer())
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