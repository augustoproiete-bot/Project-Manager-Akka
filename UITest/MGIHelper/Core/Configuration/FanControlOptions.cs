using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace MGIHelper.Core.Configuration
{
    public sealed class FanControlOptions : INotifyPropertyChanged
    {
        private readonly string _path;
        private Dictionary<string, string> _dictionary;

        private Dictionary<string, string> Dictionary => _dictionary;

        public FanControlOptions(string path)
        {
            _path = path;

            _dictionary = new Dictionary<string, string>();
        }

        private TValue GetValue<TValue>(Func<string, TValue> converter, TValue defaultValue = default, [CallerMemberName]string name = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) return default;

                return Dictionary.TryGetValue(name, out var value) ? converter(value) : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        private void SetValue(string value, [CallerMemberName] string name = null)
        {
            if(string.IsNullOrEmpty(name)) return;

            Dictionary[name] = value;

            OnPropertyChanged(name);
        }

        public int ClockTimeMs
        {
            get => GetValue(int.Parse, 1000);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public string Ip
        {
            get => GetValue(s => s, "192.168.187.48");
            set => SetValue(value);
        }

        public int GoStandbyTime
        {
            get => GetValue(int.Parse, 25);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStartupTemp
        {
            get => GetValue(int.Parse, 70);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStandbyTemp
        {
            get => GetValue(int.Parse, 115);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double FanControlMultipler
        {
            get => GetValue(double.Parse, 1.3d);
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public async Task Load()
        {

            try
            {
                if (File.Exists(_path)) return;

                //Interlocked.Exchange(ref _dictionary, JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_path)));

                await using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read);

                var dic = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream);
                Interlocked.Exchange(ref _dictionary, dic);
            }
            catch
            {
                Interlocked.Exchange(ref _dictionary, new Dictionary<string, string>());
            }

        }

        public async Task Save() 
            => await File.WriteAllBytesAsync(_path, JsonSerializer.SerializeToUtf8Bytes(_dictionary));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}