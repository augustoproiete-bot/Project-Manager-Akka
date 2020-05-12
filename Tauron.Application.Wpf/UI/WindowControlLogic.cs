using System.Windows;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.UI
{
    public sealed class WindowControlLogic : ControlLogicBase<Window>
    {
        public WindowControlLogic([NotNull] Window userControl, [NotNull] IViewModel model) : base(userControl, model)
        {
            userControl.SizeToContent = SizeToContent.Manual;
            userControl.ShowInTaskbar = true;
            userControl.ResizeMode = ResizeMode.CanResize;
            userControl.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void WireUpUnloaded()
        {
            UserControl.Closed += (sender, args) => UserControlOnUnloaded();
        }
    }
}