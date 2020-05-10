using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Akka.Event;
using Autofac;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Akka.MGIHelper.UI
{
    public sealed class LogWindowViewModel : UiActor
    {
        private UIObservableCollection<string>? UnhandledMessages
        {
            get => Get<UIObservableCollection<string>>();
            set => Set(value);
        }

        public LogWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher) 
            : base(lifetimeScope, dispatcher)
        {
            UnhandledMessages = new UIObservableCollection<string> {"Start"};
            Receive<UnhandledMessage>(NewUnhandledMessage);
            Context.System.EventStream.Subscribe(Context.Self, typeof(UnhandledMessage));
        }

        private void NewUnhandledMessage(UnhandledMessage obj)
        {
            var builder = new StringBuilder($"Name: {obj.GetType().Name}");

            foreach (var propertyInfo in obj.Message.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)) 
                builder.Append($" - {propertyInfo.GetValue(obj.Message)}");

            UnhandledMessages?.Add(builder.ToString());
        }
    }
}