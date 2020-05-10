using System.Windows.Controls;
using Tauron.Application.Wpf;

namespace Akka.MGIHelper.UI.FanControl
{
    /// <summary>
    /// Interaktionslogik für AutoFanControl.xaml
    /// </summary>
    public partial class AutoFanControl
    {
        public AutoFanControl(IViewModel<AutoFanControlModel> model)
            : base(model)
        {
            InitializeComponent();
        }
    }
}
