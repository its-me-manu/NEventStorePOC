
namespace NEventStorePOC.Model
{
    public class EventStream
    {
        public DateTimeOffset CommitStamp { get; set;  }
        public Guid CommitId { get; set; }

        public int EventId { get; set; }
        public string EventType { get; set; }
        public string EventData { get; set; }
    }
}
