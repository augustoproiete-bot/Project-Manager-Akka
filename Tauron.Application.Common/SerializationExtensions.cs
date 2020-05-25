using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Optional;
using Optional.Linq;

namespace Tauron
{
    [PublicAPI]
    public static class SerializationExtensions
    {
        private class XmlSerilalizerDelegator : IFormatter
        {
            private readonly XmlSerializer _serializer;

            public XmlSerilalizerDelegator(XmlSerializer serializer) => _serializer = serializer;

            public SerializationBinder? Binder
            {
                get => null;

                set { }
            }

            public StreamingContext Context
            {
                get => new StreamingContext();

                set { }
            }

            public ISurrogateSelector? SurrogateSelector
            {
                get => null;

                set { }
            }
            
            public object Deserialize(Stream serializationStream) => _serializer.Deserialize(serializationStream);

            public void Serialize(Stream serializationStream, object graph) => _serializer.Serialize(serializationStream, graph);
        }

        private static Option<IFormatter> _binaryFormatter = Option.Some<IFormatter>(new BinaryFormatter());

        public static Option<TValue> Deserialize<TValue>(this Option<string> path, Option<IFormatter> formatter)
            where TValue : class
        {
            var stream = path.OpenRead();
            var result = InternalDeserialize(formatter, stream);
            stream.MatchSome(s => s.Dispose());

            return from obj in result
                   where obj is TValue
                   select (TValue) obj;
        }

        public static Option<TValue> Deserialize<TValue>(this Option<string> path)
            where TValue : class =>
            Deserialize<TValue>(path, _binaryFormatter);

        public static Option<bool> Serialize(this Option<object> graph, Option<IFormatter> formatter, Option<string> path)
        {
            var stream = path.OpenWrite();
            var result = InternalSerialize(graph, formatter, stream);
            stream.MatchSome(s => s.Dispose());
            return result;
        }

        public static Option<bool> Serialize(this Option<object> graph, Option<string> path) 
            => Serialize(graph, _binaryFormatter, path);

        public static Option<TValue> XmlDeserialize<TValue>(this Option<string> path, Option<XmlSerializer> formatter)
            where TValue : class
        {
            var stream = path.SomeWhen(o => o.ExisFile().ValueOr(false)).Flatten().OpenRead();
            var result = InternalDeserialize(formatter.Map<IFormatter>(s => new XmlSerilalizerDelegator(s)), stream);
            stream.MatchSome(s => s.Dispose());

            return from obj in result
                   where obj is TValue
                   select (TValue)obj;
        }

        public static Option<bool> XmlSerialize(this Option<object> graph, Option<XmlSerializer> formatter, Option<string> path)
        {
            var stream = path.OpenWrite();
            var result = InternalSerialize(graph, formatter.Map<IFormatter>(x => new XmlSerilalizerDelegator(x)), stream);
            stream.MatchSome(s => s.Dispose());
            return result;
        }

        
        private static Option<object> InternalDeserialize(Option<IFormatter> formatter, Option<Stream> stream)
            => from f in formatter
               from s in stream
               select f.Deserialize(s);

        private static Option<bool> InternalSerialize(Option<object> graph, Option<IFormatter> formatter, Option<Stream> stream)
        {
            var data = from obj in graph
                         from form in formatter
                         from str in stream
                         select (form, str, obj);

            return data.Map(r =>
                            {
                                r.form.Serialize(r.str, r.obj);
                                return true;
                            });
        }
    }
}