using System;
using System.Globalization;
using System.Windows.Threading;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;

namespace Tauron.Application.Wpf.UI
{
    public sealed class ViewConnector : ModelConnectorBase<ViewConnector>
    {
        private readonly Dispatcher _dispatcher;
        private readonly ViewManager _manager;
        private readonly IView _root;
        private readonly Action<object> _updater;
        private readonly string _viewModelKey = Guid.NewGuid().ToString();

        public ViewConnector(string name, DataContextPromise promise, ViewManager manager, IView root, Action<object> updater, Dispatcher dispatcher)
            : base(name, promise)
        {
            _manager = manager;
            _root = root;
            _updater = updater;
            _dispatcher = dispatcher;
        }

        protected override void OnLoad()
        {
            _manager.RegisterConnector(_viewModelKey, this);
        }

        protected override void OnUnload()
        {
            _manager.UnregisterConnector(_viewModelKey);
        }

        protected override void ValidateCompled(ValidatingEvent obj)
        {
        }

        protected override void PropertyChangedHandler(PropertyChangedEvent obj)
        {
            var converter = new ViewModelConverter();
            if (!(obj.Value is IViewModel viewModel)) return;

            var view = _dispatcher.Invoke(() => converter.Convert(viewModel, GetType(), _root, CultureInfo.CurrentUICulture) as IView);
            if (view == null) return;

            _updater(view);
        }

        public override string ToString()
        {
            return "View Connector Loading...";
        }
    }
}