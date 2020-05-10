using System.Windows;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using CommandManager = System.Windows.Input.CommandManager;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public class UserControl : System.Windows.Controls.UserControl, IView
    {
        private sealed class UserControlLogic
        {
            private readonly UserControl _userControl;
            private readonly IViewModel _model;

            public UserControlLogic(UserControl userControl, IViewModel model)
            {
                _userControl = userControl;
                _model = model;

                userControl.Loaded += UserControlOnLoaded;
                userControl.Unloaded += UserControlOnUnloaded;
            }

            private void UserControlOnUnloaded(object sender, RoutedEventArgs e)
            {
                _model.Tell(new UnloadEvent());
                _model.Reset();
            }

            private void UserControlOnLoaded(object sender, RoutedEventArgs e)
            {
                if (!_model.IsInitialized)
                {
                    var parent = ControlBindLogic.FindParentDatacontext(_userControl);
                    if (parent != null)
                        parent.Tell(new InitParentViewModel(_model));
                    else
                        _model.Init();
                }

                _model.Tell(new InitEvent());
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private readonly ControlBindLogic _controlBindLogic;
        // ReSharper disable once NotAccessedField.Local
        private readonly UserControlLogic _controlLogic;

        protected UserControl(IViewModel viewModel)
        {
            _controlLogic = new UserControlLogic(this, viewModel);
            _controlBindLogic = new ControlBindLogic(this, viewModel);
            DataContextChanged += (sender, args) =>
                                  {
                                      if (args.NewValue != viewModel)
                                          ((FrameworkElement) sender).DataContext = viewModel;
                                  };
        }

        void IBinderControllable.Register(string key, IControlBindable bindable, DependencyObject affectedPart)
        {
            _controlBindLogic.Register(key, bindable, affectedPart);
            CommandManager.InvalidateRequerySuggested();
        }

        public void CleanUp(string key) 
            => _controlBindLogic.CleanUp(key);
    }
}