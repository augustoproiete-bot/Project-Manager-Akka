using System.Windows;
using Tauron.Application.Wpf.Helper;
using CommandManager = System.Windows.Input.CommandManager;

namespace Tauron.Application.Wpf
{
    public class UserControl : System.Windows.Controls.UserControl, IBinderControllable
    {
        private sealed class UserControlLogic
        {
            
        }

        private readonly ControlBindLogic _controlLogic;

        protected UserControl(IViewModel viewModel)
        {
            _controlLogic = new ControlBindLogic(this, viewModel);
            DataContextChanged += (sender, args) =>
                                  {
                                      if (args.NewValue != viewModel)
                                          ((FrameworkElement) sender).DataContext = viewModel;
                                  };
        }

        void IBinderControllable.Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            _controlLogic.Register(key, bindable, affectedPart);
            CommandManager.InvalidateRequerySuggested();
        }

        public void CleanUp(string key) 
            => _controlLogic.CleanUp(key);
    }
}