using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Akka.Actor;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.ServiceManager.Views.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AddSeedDialog.xaml
    /// </summary>
    public partial class AddSeedDialog : IAddSeedUrlDialog
    {
        private readonly ActorSystem _system;

        public AddSeedDialog(ActorSystem system)
        {
            _system = system;
            InitializeComponent();
            Background = Brushes.Transparent;
        }

        public Task<DialogSeedEntry> Init(IEnumerable<DialogSeedEntry> initalData)
            => MakeTask<DialogSeedEntry>(s => new AddSeedDialogModel(initalData, u => s.SetResult(new DialogSeedEntry(u)), _system.Name));
    }


    public sealed class AddSeedDialogModel : ObservableObject
    {
        private string? _newUrl;
        private bool _isloading;
        //private readonly Probe _probe;

        public AddSeedDialogModel(IEnumerable<DialogSeedEntry> knowenUrls, Action<string?> setUrl, string systemName)
        {
            //_probe = new Probe(systemName);
            //_probe.BeaconsUpdated += ProbeOnBeaconsUpdated; 
            //_probe.Start();

            var urls = knowenUrls.Select(se => se.Url).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()!;

            NewUrl = null;
            OkCommand = new SimpleCommand(() => urls.All(s => s != _newUrl),
                () =>
                {
                    //_probe.Dispose();
                    setUrl(NewUrl);
                });
        }

        //private void ProbeOnBeaconsUpdated(IEnumerable<BeaconLocation> obj)
        //{
        //    _probe.Stop();

        //    foreach (var location in obj) 
        //        Suggest.Add(location.Data);

        //    Isloading = false;
        //}

        public bool Isloading
        {
            get => _isloading;
            set
            {
                if (value == _isloading) return;
                _isloading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Suggest { get; } = new UIObservableCollection<string>();

        public ICommand OkCommand { get; }

        public string? NewUrl
        {
            get => _newUrl;
            set
            {
                if (value == _newUrl) return;
                _newUrl = value;
                OnPropertyChanged();
            }
        }
    }
}
