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
        private readonly Action<object> _updater;
        private readonly string _viewModelKey = Guid.NewGuid().ToString();

        private ViewManager _manager = ViewManager.Manager;

        public ViewConnector(string name, DataContextPromise promise, Action<object> updater, Dispatcher dispatcher)
            : base(name, promise)
        {
            _updater = updater;
            _dispatcher = dispatcher;
        }

        protected override void OnLoad()
        {
            if(View == null) return;
            _manager = View.ViewManager;
            _manager.RegisterConnector(_viewModelKey, this);
        }

        protected override void OnUnload()
        {
            if(View == null) return;
            _manager.UnregisterConnector(_viewModelKey);
        }

        protected override void NoDataContextFound() => _updater($"No Data Context Found for {Name}");

        protected override void ValidateCompled(ValidatingEvent obj)
        {
        }

        protected override void PropertyChangedHandler(PropertyChangedEvent obj)
        {
            if(View == null) return;

            var converter = new ViewModelConverter();
            if (!(obj.Value is IViewModel viewModel)) return;

            var view = _dispatcher.Invoke(() => converter.Convert(viewModel, GetType(), View, CultureInfo.CurrentUICulture) as IView);
            if (view == null) return;

            _updater(view);
        }

        public override string ToString() => "View Connector Loading...";
    }
}