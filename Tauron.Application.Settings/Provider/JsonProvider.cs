using System.Collections.Immutable;
using System.IO;
using Functional.Maybe;
using JetBrains.Annotations;
using Newtonsoft.Json;
using static Tauron.Prelude;

namespace Tauron.Application.Settings.Provider
{
    [PublicAPI]
    public sealed class JsonProvider : ISettingProvider
    {
        private readonly Maybe<string> _fileName;

        public JsonProvider(Maybe<string> fileName)
        {
            Do(from dic in IO.Path.GetDirectoryName(IO.Path.GetFullPath(fileName))
                where !Directory.Exists(dic)
               select Directory.CreateDirectory(dic));

            _fileName = IO.Path.GetFullPath(fileName);
        }

        public Maybe<ImmutableDictionary<string, string>> Load()
            =>  from exists in IO.File.Exists(_fileName)
                where exists
                from file in _fileName
                select JsonConvert.DeserializeObject<ImmutableDictionary<string, string>>(File.ReadAllText(file));

        public Maybe<ImmutableDictionary<string, string>> Save(ImmutableDictionary<string, string> data)
            => from file in _fileName
                select Func(() =>
                {
                    File.WriteAllText(file, JsonConvert.SerializeObject(data));
                    return data;
                });
    }
}