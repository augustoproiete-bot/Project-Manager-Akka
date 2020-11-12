using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Autofac;
using Functional.Maybe;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tauron.Localization.Actor
{
    [UsedImplicitly]
    public sealed class JsonLocLocStoreActor : LocStoreActorBase
    {
        private static readonly char[] Sep = {'.'};

        private readonly Maybe<JsonConfiguration> _configuration;
        private readonly Dictionary<string, Dictionary<string, JToken>> _files = new ();
        private bool _isInitialized;

        public JsonLocLocStoreActor(ILifetimeScope scope)
        {
            _configuration = Maybe.NotNull(scope.ResolveOptional<JsonConfiguration>());

            Self.Tell(new BeginInit());
            Receive<BeginInit>(EnsureInitialized);
        }

        protected override Maybe<object> TryQuery(string name, CultureInfo target)
        {
            var fallBack = GetConfig(j => j.Fallback);

            var data = from cultureInfo in target.ToMaybe()
                    .Flatten(info => info.Parent.Equals(CultureInfo.InvariantCulture) ? Maybe<CultureInfo>.Nothing : info.Parent.ToMaybe())
                    select LookUp(name, cultureInfo);

            return data.FirstOrDefault(m => m.HasValue)
                .Or((from info in fallBack select LookUp(name, CultureInfo.GetCultureInfo(info))).Collapse());
        }

        private Maybe<object> LookUp(string name, CultureInfo target)
        {
            var language = 
                from mode in GetConfig(c => c.NameMode)
                select mode switch
                {
                    JsonFileNameMode.Name => target.Name,
                    JsonFileNameMode.TwoLetterIsoLanguageName => target.TwoLetterISOLanguageName,
                    JsonFileNameMode.ThreeLetterIsoLanguageName => target.ThreeLetterISOLanguageName,
                    JsonFileNameMode.ThreeLetterWindowsLanguageName => target.ThreeLetterWindowsLanguageName,
                    JsonFileNameMode.DisplayName => target.DisplayName,
                    JsonFileNameMode.EnglishName => target.EnglishName,
                    _ => throw new InvalidOperationException("No Valid Json File Name Mode")
                };

            var result =
                from lang in language
                from data in _files.Lookup(lang)
                from entry in data.Lookup(name)
                select Maybe.NotNull(entry as JValue);


            return
                from value in result.Collapse()
                select Maybe.NotNull(value.Type == JTokenType.String ? EscapeHelper.Decode(value.Value<string>()) : value.Value);
        }

        private Maybe<TValue> GetConfig<TValue>(Func<JsonConfiguration, TValue> convert)
            => from config in _configuration
                select convert(config);

        private void EnsureInitialized(BeginInit trigger)
        {
            if (_isInitialized) return;

            var rootDic = GetConfig(j => j.RootDic);
            rootDic.Do(_ => _files.Clear());

            var data =
                from dic in rootDic
                select
                    from file in Directory.EnumerateFiles(dic, "*.json")
                    select
                        from text in File.ReadAllText(file, Encoding.UTF8).ToMaybe()
                        let mayName = GetName(file)
                        from name in mayName
                        select new {Name = name, Data = JsonConvert.DeserializeObject<Dictionary<string, JToken>>(text)};

            data.Do(d =>
            {
                foreach (var readData in from e in d
                                     where e.HasValue
                                     select e.Value) 
                    _files.Add(readData.Name, readData.Data);
            });

            _isInitialized = true;
        }

        private static Maybe<string> GetName(string fileName)
        {
            var data = Path.GetFileNameWithoutExtension(fileName).Split(Sep, StringSplitOptions.RemoveEmptyEntries);
            return Maybe.NotNull(data.Length switch
            {
                2 => data[1],
                1 => data[0],
                _ => null
            });
        }

        private sealed record BeginInit;
    }
}