using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using JetBrains.Annotations;
using Serilog;
using Tauron.Application.Master.Commands.Administration.Host;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.ServiceManager.Views.Dialogs
{
    /// <summary>
    /// Interaktionslogik für SelectHostAppDialog.xaml
    /// </summary>
    public partial class SelectHostAppDialog : ISelectHostAppDialog
    {
        public SelectHostAppDialog()
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }

        public Task<HostApp?> Init(Task<HostApp[]> initalData) 
            => MakeTask<HostApp?>(t => new SelectHostAppViewModel(initalData, t));

        private void Control_OnMouseDoubleClick(object sender, MouseButtonEventArgs e) => ((ListBox)sender).UnselectAll();
    }

    public sealed class SelectHostAppViewModel : ObservableObject
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<SelectHostAppViewModel>();

        private HostApp? _selectted;
        private bool _loading;
        private string? _info;
        private IEnumerable<HostApp>? _apps;

        public SelectHostAppViewModel(Task<HostApp[]> app, TaskCompletionSource<HostApp?> select)
        {
            Loading = true;
            app.ContinueWith(t =>
            {
                Loading = false;

                try
                {
                    Apps = t.Result;
                }
                catch (Exception e)
                {
                    Log.Error(e, "Error on Loading Host Apps Data");
                    Info = e.Unwrap()?.Message;
                }
            });

            Select = new SimpleCommand(() => Selectted != null, () => select.SetResult(Selectted));
            Cancel = new SimpleCommand(() => select.SetResult(null));
        }

        [UsedImplicitly]
        public SelectHostAppViewModel() 
            : this(Task.FromResult(new [] { new HostApp("Test", "Test Pfad", 5, AppType.Cluster, true, "hallo.exe", true) }), new TaskCompletionSource<HostApp?>())
        {
            
        }

        public IEnumerable<HostApp>? Apps
        {
            get => _apps;
            set
            {
                if (Equals(value, _apps)) return;
                _apps = value;
                OnPropertyChanged();
            }
        }

        public HostApp? Selectted
        {
            get => _selectted;
            set
            {
                if (Equals(value, _selectted)) return;
                _selectted = value;
                OnPropertyChanged();
            }
        }

        [UsedImplicitly]
        public bool Loading
        {
            get => _loading;
            set
            {
                if (value == _loading) return;
                _loading = value;
                OnPropertyChanged();
            }
        }

        public string? Info
        {
            get => _info;
            set
            {
                if (value == _info) return;
                _info = value;
                OnPropertyChanged();
            }
        }

        public ICommand Select { get; }

        public ICommand Cancel { get; }
    }
}
