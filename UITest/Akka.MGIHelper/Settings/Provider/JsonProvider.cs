﻿using System.Collections.Immutable;
using System.IO;
using Newtonsoft.Json;

namespace Akka.MGIHelper.Settings.Provider
{
    public sealed class JsonProvider : ISettingProvider
    {
        private readonly string _fileName;

        public JsonProvider(string fileName)
        {
            _fileName = fileName;
        }

        public ImmutableDictionary<string, string> Load()
        {
            return File.Exists(_fileName) ? JsonConvert.DeserializeObject<ImmutableDictionary<string, string>>(File.ReadAllText(_fileName)) : ImmutableDictionary<string, string>.Empty;
        }

        public void Save(ImmutableDictionary<string, string> data)
        {
            File.WriteAllText(_fileName, JsonConvert.SerializeObject(data));
        }
    }
}