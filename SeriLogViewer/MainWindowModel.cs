using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using Ookii.Dialogs.Wpf;
using Syncfusion.UI.Xaml.Grid;

namespace SeriLogViewer
{
    public class MainWindowModel : INotifyPropertyChanged
    {
        private readonly SfDataGrid _grid;
        public event PropertyChangedEventHandler? PropertyChanged;

        private int _isLoading = 0;
        private ObservableCollection<dynamic> _entrys = new ObservableCollection<dynamic>();

        public ICommand OpenCommand { get; }

        public MainWindowModel(SfDataGrid grid)
        {
            _grid = grid;
            OpenCommand = new SimpleCommand(() => _isLoading == 0, TryLoad);
        }

        private void TryLoad()
        {
            var diag = new VistaOpenFileDialog();
            if(diag.ShowDialog(Application.Current.MainWindow) != true) return;

            Task.Run(() => Load(diag.FileName));
        }

        private void Load(string file)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() => Entrys.Clear());

                List<dynamic> entrys = new List<dynamic>(); 

                Interlocked.Exchange(ref _isLoading, 1);

                using var reader = new StreamReader(file);
                string? line = string.Empty;

                while ((line = reader.ReadLine()) != null)
                {
                    dynamic target = JToken.Parse(line);
                    ReconstructMessage(target);
                    entrys.Add(target);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (dynamic entry in entrys)
                    {
                        Entrys.Add(entry);
                    }

                    _grid.GridColumnSizer.ResetAutoCalculationforAllColumns();
                    _grid.GridColumnSizer.Refresh();
                });
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                Interlocked.Exchange(ref _isLoading, 0);
            }
        }

        private void ReconstructMessage(JToken obj)
        {
            try
            {
                string msg = obj["@mt"]!.Value<string>();
                var prov = obj;

                foreach (var match in FindComponent(msg))
                {
                    msg = msg.Replace(match, prov[match[1..^1]]?.ToString() ?? string.Empty);
                }

                obj["@mt"] = JToken.FromObject(msg);
            }
            catch
            {
                // ignored
            }
        }

        private IEnumerable<string> FindComponent(string msg)
        {
            var target = msg;
            var isIn = false;
            var start = 0;

            for (var i = 0; i < msg.Length; i++)
            {
                if (isIn && target[i] == '}')
                {
                    isIn = false;
                    yield return target.Substring(start, i + 1 - start);
                }
                else if (!isIn && msg[i] == '{')
                {
                    isIn = true;
                    start = i;
                }
            }
        }

        public ObservableCollection<dynamic> Entrys
        {
            get => _entrys;
            set
            {
                if (Equals(value, _entrys)) return;
                _entrys = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}