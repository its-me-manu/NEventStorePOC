using NEventStore.Serialization;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NEventStorePOC.Framework
{
    public class JsonSerializer : ISerialize
    {
        private readonly ISerialize _inner;
        private readonly Newtonsoft.Json.JsonSerializer _serializer = new Newtonsoft.Json.JsonSerializer
                                                                      {
                                                                          TypeNameHandling = TypeNameHandling.All,
                                                                          DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                          NullValueHandling = NullValueHandling.Ignore,
                                                                          ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                                                                      };

        private readonly Newtonsoft.Json.JsonSerializer _deserializer = new Newtonsoft.Json.JsonSerializer
                                                                        {
                                                                            TypeNameHandling = TypeNameHandling.All,
                                                                            DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                            NullValueHandling = NullValueHandling.Ignore,
                                                                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                                        };

        public virtual void Serialize<T>(Stream output, T graph)
        {
            using (StreamWriter streamWriter = new StreamWriter(output, Encoding.UTF8))
               this.Serialize((JsonWriter) new JsonTextWriter((TextWriter) streamWriter), (object) graph);

            //using (var compress = new DeflateStream(output, CompressionMode.Compress, true))
                //_inner.Serialize(compress, graph);

        }

        public virtual T Deserialize<T>(Stream input)
        {
            using (StreamReader streamReader = new StreamReader(input, Encoding.UTF8))
                return Deserialize<T>((JsonReader) new JsonTextReader((TextReader) streamReader));

            //using (var decompress = new DeflateStream(input, CompressionMode.Decompress, true))
                //return _inner.Deserialize<T>(decompress);
        }

        protected virtual void Serialize(JsonWriter writer, object graph)
        {
            using (writer)
                _serializer.Serialize(writer, graph);
        }

        protected virtual T Deserialize<T>(JsonReader reader)
        {
            Type type = typeof(T);
            using (reader)
                return (T) _deserializer.Deserialize(reader, type);
        }
    }

    abstract class JsonConverterToHandleVersionChanges<T> : JsonConverter
    {
         private Newtonsoft.Json.JsonSerializer _deserializer = new Newtonsoft.Json.JsonSerializer
                                                               {
                                                                   TypeNameHandling = TypeNameHandling.All,
                                                                   DefaultValueHandling = DefaultValueHandling.Ignore,
                                                                   NullValueHandling = NullValueHandling.Ignore,
                                                                   ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                               };

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            using (var readerForDeserialization = jsonObject.CreateReader())
            {
                var result = _deserializer.Deserialize<T>(readerForDeserialization);
                HandleChangedFieldNames(jsonObject, result);
                return result;
            }

        }

        protected abstract void HandleChangedFieldNames(JObject jsonObject, T result);

        protected void HandleChangedFieldName<TResult, TPropertyType>(JObject jsonObject,
                                                                      TResult result,
                                                                      string oldPropertyName,
                                                                      Action<TResult, TPropertyType> propertySetter)
        {
            JToken token = null;
            var hasText = jsonObject?.TryGetValue(oldPropertyName, out token);
            if (hasText.GetValueOrDefault())
            {
                propertySetter(result, token.Value<TPropertyType>());
            }
        }
    }

}