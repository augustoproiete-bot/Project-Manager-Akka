using Tauron.Application.Wpf;

namespace Akka.MGIHelper.UI.MgiStarter
{
    /// <summary>
    ///     Interaktionslogik für MgiStarterControl.xaml
    /// </summary>
    public partial class MgiStarterControl
    {
        public MgiStarterControl(IViewModel<MgiStarterControlModel> model)
            : base(model)
        {
            InitializeComponent();
        }
    }
}