using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.ServiceManager.ViewModels.Dialogs;

namespace Tauron.Application.ServiceManager.Views.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AddSeedDialog.xaml
    /// </summary>
    public partial class AddSeedDialog : IAddSeedUrlDialog
    {
        public AddSeedDialog()
        {
            InitializeComponent();
        }

        public Task<DialogSeedEntry> Init(IEnumerable<DialogSeedEntry> initalData)
            => MakeTask<DialogSeedEntry>(s => new AddSeedDialogModel(initalData, u => s.SetResult(new DialogSeedEntry(u))));
    }

    public sealed class AddSeedDialogModel
    {
        private readonly Action<string> _setUrl;
        private readonly string[] _urls;

        public AddSeedDialogModel(IEnumerable<DialogSeedEntry> knowenUrls, Action<string> setUrl)
        {
            _setUrl = setUrl;
            _urls = knowenUrls.Select(se => se.Url).ToArray();
        }
    }
}
