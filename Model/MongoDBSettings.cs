namespace NEventStorePOC.Model;
public class MongoDBSettings
{
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
    public string EventStoreDB { get; set; }
}

public class SQLDBSettings
{
    public string ConnectionString { get; set; }
}