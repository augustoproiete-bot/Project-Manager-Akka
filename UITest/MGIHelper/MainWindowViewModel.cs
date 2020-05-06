using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MGIHelper.Core.Configuration;

namespace MGIHelper
{
    public sealed class MainWindowViewModel
    {
        private readonly object _gate = new object();

        public WindowOptions WindowOptions { get; }
        public MainWindowViewModel()
        {
            WindowOptions = new WindowOptions(Path.Combine(AppDomain.CurrentDomain.BaseDirectory ?? throw new InvalidOperationException(), "WindowSettings.json"));

            WindowOptions.PropertyChanged += WindowOptionsOnPropertyChanged;
        }

        private async void WindowOptionsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!Monitor.TryEnter(_gate, 1000)) return;

            try
            {
                await WindowOptions.Save();
            }
            catch (Exception ee)
            {

            }
            finally
            {
                Monitor.Exit(_gate);
            }
        }

        public async Task Init() 
            => await WindowOptions.Load();
    }
}