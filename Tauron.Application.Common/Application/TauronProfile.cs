using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Serilog;

namespace Tauron.Application
{
    [PublicAPI]
    public abstract class TauronProfile : ObservableObject, IEnumerable<string>
    {
        private static readonly char[] ContentSplitter = {'='};

        private readonly string _defaultPath;
        private readonly ILogger _logger = Log.ForContext<TauronProfile>();

        private readonly Dictionary<string, string> _settings = new();

        protected TauronProfile(string application, string defaultPath)
        {
            Application = Argument.NotNull(application, nameof(application));
            _defaultPath = Argument.NotNull(defaultPath, nameof(defaultPath));
        }

        public virtual string this[[NotNull] string key]
        {
            get => _settings[key];

            set
            {
                IlligalCharCheck(key);
                _settings[key] = value;
            }
        }

        public int Count => _settings.Count;

        public string Application { get; private set; }

        public string? Name { get; private set; }

        protected string? Dictionary { get; private set; }

        protected string? FilePath { get; private set; }

        public IEnumerator<string> GetEnumerator() 
            => _settings.Select(k => k.Key).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public void Delete()
        {
            _settings.Clear();

            _logger.Information($"{Application} -- Delete Profile infos... {Dictionary?.PathShorten(20)}");

            Dictionary?.DeleteDirectory();
        }

        public virtual void Load([NotNull] string name)
        {
            Argument.NotNull<object>(name, nameof(name));
            IlligalCharCheck(name);

            Name = name;
            Dictionary = _defaultPath.CombinePath(Application, name);
            Dictionary.CreateDirectoryIfNotExis();
            FilePath = Dictionary.CombinePath("Settings.db");

            _logger.Information($"{Application} -- Begin Load Profile infos... {FilePath.PathShorten(20)}");

            _settings.Clear();
            foreach (var vals in
                FilePath.EnumerateTextLinesIfExis()
                    .Select(line => line.Split(ContentSplitter, 2))
                    .Where(vals => vals.Length == 2))
            {
                _logger.Information("key: {0} | Value {1}", vals[0], vals[1]);

                _settings[vals[0]] = vals[1];
            }
        }

        public virtual void Save()
        {
            _logger.Information($"{Application} -- Begin Save Profile infos...");

            try
            {
                using var writer = FilePath?.OpenTextWrite();

                if (writer == null) return;

                foreach (var (key, value) in _settings)
                {
                    writer.WriteLine("{0}={1}", key, value);

                    _logger.Information("key: {0} | Value {1}", key, value);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Profile Save");
            }
        }

        public virtual string? GetValue(string? defaultValue, [CallerMemberName] string? key = null)
        {
            var cKey = Argument.NotNull(key, nameof(key));

            IlligalCharCheck(cKey);

            return !_settings.ContainsKey(cKey) ? defaultValue : _settings[cKey];
        }

        public virtual int GetValue(int defaultValue, [CallerMemberName] string? key = null) 
            => int.TryParse(GetValue(null, key), out var result) ? result : defaultValue;

        public virtual bool GetValue(bool defaultValue, [CallerMemberName] string? key = null) 
            => bool.TryParse(GetValue(null, key), out var result) ? result : defaultValue;

        public virtual void SetVaue(object value, [CallerMemberName] string? key = null)
        {
            var cKey = Argument.NotNull(key, nameof(key));
            Argument.NotNull(value, nameof(value));
            IlligalCharCheck(cKey);

            _settings[cKey] = value.ToString() ?? string.Empty;
            OnPropertyChangedExplicit(cKey);
        }

        private static void IlligalCharCheck(string key) 
            => Argument.Check(key.Contains('='), () => new ArgumentException($"The Key ({key}) Contains an Illigal Char: ="));

        public void Clear() 
            => _settings.Clear();
    }
}