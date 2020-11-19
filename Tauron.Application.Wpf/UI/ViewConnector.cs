using System;
using System.Globalization;
using System.Windows.Threading;
using Functional.Maybe;
using Tauron.Application.Wpf.Helper;
using Tauron.Application.Wpf.ModelMessages;
using static Tauron.Prelude;

namespace Tauron.Application.Wpf.UI
{
    public sealed class ViewConnector : ModelConnectorBase<ViewConnector>
    {
        private static readonly ViewModelConverter Converter = new();
        
        private readonly Dispatcher     _dispatcher;
        private readonly Action<object> _updater;
        private readonly string         _viewModelKey = Guid.NewGuid().ToString();

        private Maybe<ViewManager> _manager = May(ViewManager.Manager);

        public ViewConnector(string name, DataContextPromise promise, Action<object> updater, Dispatcher dispatcher)
            : base(name, promise)
        {
            _updater    = updater;
            _dispatcher = dispatcher;
        }

        protected override void OnLoad()
        {
            _manager = from view in View
                       select Func(() =>
                                   {
                                       var manager = view.ViewManager;
                                       manager.RegisterConnector(_viewModelKey, this);
                                       return manager;
                                   });

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            Do(from _ in View
               from manager in _manager 
               select Action(() => manager.UnregisterConnector(_viewModelKey)));

            base.OnUnload();
        }

        protected override void NoDataContextFound() => _updater($"No Data Context Found for {Name}");

        protected override void ValidateCompled(ValidatingEvent obj)
        {
        }

        protected override void PropertyChangedHandler(PropertyChangedEvent obj)
        {
            Do(from _ in View
               from value in obj.Value
               where value is IViewModel
               select Action(() =>
                             {
                                 var view = _dispatcher.Invoke(() => Converter.Convert((IViewModel) value, GetType(), View, CultureInfo.CurrentUICulture) as IView);
                                 if (view == null) return;

                                 _updater(view);
                             }));
        }

        public override string ToString() => "View Connector Loading...";
    }
}