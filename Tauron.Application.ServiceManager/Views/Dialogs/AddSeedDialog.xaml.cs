using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using Akka.Actor;
using BeaconLib;
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
        private bool _isloading = true;

        public AddSeedDialogModel(IEnumerable<DialogSeedEntry> knowenUrls, Action<string?> setUrl, string systemName)
        {
            var probe = new Probe(systemName);
            probe.BeaconsUpdated += ProbeOnBeaconsUpdated; 
            probe.Start();

            var urls = knowenUrls.Select(se => se.Url).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray()!;

            NewUrl = null;
            OkCommand = new SimpleCommand(() => urls.All(s => s != _newUrl),
                () =>
                {
                    probe.Dispose();
                    setUrl(NewUrl);
                });
        }

        private void ProbeOnBeaconsUpdated(IEnumerable<BeaconLocation> obj)
        {
            var set = new HashSet<string>(Suggest);
            var set2 = new HashSet<string>();

            foreach (var location in obj)
            {
                if(set2.Add(location.Data) && set.Add(location.Data))
                    Suggest.Add(location.Data);
            }

            foreach (var sug in Suggest.ToArray())
            {
                if (set.Add(sug))
                    Suggest.Remove(sug);
            }

            Isloading = false;
        }

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
