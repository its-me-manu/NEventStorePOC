using NEventStore;

namespace NEventStorePOC.Framework
{
     public static class BsonSerializationWireupExtension
    {
        public static SerializationWireup UsingBsonSerialization(this PersistenceWireup wireup)
        {
            return wireup.UsingCustomSerialization(new JsonSerializer());
        }
    }

}