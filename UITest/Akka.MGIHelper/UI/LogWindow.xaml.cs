using Tauron.Application.Wpf;

namespace Akka.MGIHelper.UI
{
    /// <summary>
    ///     Interaktionslogik für LogWindow.xaml
    /// </summary>
    public partial class LogWindow
    {
        public LogWindow(IViewModel<LogWindowViewModel> model)
            : base(model)
        {
            InitializeComponent();
        }
    }
}