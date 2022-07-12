using System.Transactions;
using NEventStore;
using System.Diagnostics;
using NEventStore.Persistence;
using NEventStore.Serialization;

namespace NEventStorePOC.Framework
{
    // 5/9/2018 JMB: Temporarily disabling priority queue while we track down possible race conditions.
    public class DispatchToServiceBusHook : IPipelineHook
    {
        private readonly IDictionary<Type, Func<object, object>> _converters;
        private readonly IDocumentSerializer _serializer;
        private readonly ISerialize _inner;

        public DispatchToServiceBusHook()
        {

        }

        public void Dispose()
        {
            _converters.Clear();
        }

        public ICommit Select(ICommit committed)
        {
            // bool converted = false;
            // var eventMessages = committed
            //     .Events
            //     .Select(eventMessage =>
            //     {
            //         object convert = Convert(eventMessage.Body);
            //         if (ReferenceEquals(convert, eventMessage.Body))
            //         {
            //             return eventMessage;
            //         }
            //         converted = true;
            //         return new EventMessage { Headers = eventMessage.Headers, Body = convert };
            //     })
            //     .ToArray();
            // if (!converted)
            // {
            //     return committed;
            // }
            // return new Commit(committed.BucketId,
            //     committed.StreamId,
            //     committed.StreamRevision,
            //     committed.CommitId,
            //     committed.CommitSequence,
            //     committed.CommitStamp,
            //     committed.CheckpointToken,
            //     committed.Headers,
            //     eventMessages);

            return committed;
        }

        public bool PreCommit(CommitAttempt attempt)
        {
            List<EventMessage> events = new List<EventMessage>();
            foreach(var evnt in attempt.Events)
            {
                Console.WriteLine(evnt);
            }
            return true;
        }

        public void PostCommit(ICommit committed)
        {
            //var ct = new CancellationToken();
            //var task = Task.Run(async () => { await SendEventAsync(committed); }, ct);
            //task.Wait(ct);
        }

        public void OnPurge(string bucketId)
        {
        }

        public void OnDeleteStream(string bucketId, string streamId)
        {
        }

        private object Convert(object source)
        {
            if (!_converters.TryGetValue(source.GetType(), out Func<object, object> converter))
            {
                return source;
            }

            object target = converter(source);
            //Logger.LogDebug(Resources.ConvertingEvent, source.GetType(), target.GetType());

            return Convert(target);
        }
    }
}
