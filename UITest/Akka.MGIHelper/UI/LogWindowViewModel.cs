using System.Reflection;
using System.Text;
using System.Windows.Threading;
using Akka.Event;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Akka.MGIHelper.UI
{
    public sealed class LogWindowViewModel : UiActor
    {
        public LogWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher)
            : base(lifetimeScope, dispatcher)
        {
            UnhandledMessages = this.RegisterUiCollection<string>(nameof(UnhandledMessages)).AndAsync();
            UnhandledMessages.Add("Start");

            Receive<UnhandledMessage>(NewUnhandledMessage);
            Context.System.EventStream.Subscribe(Context.Self, typeof(UnhandledMessage));
        }

        private UICollectionProperty<string> UnhandledMessages { get; }

        private void NewUnhandledMessage(UnhandledMessage obj)
        {
            var builder = new StringBuilder($"Name: {obj.GetType().Name}");

            foreach (var propertyInfo in obj.Message.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                builder.Append($" - {propertyInfo.GetValue(obj.Message)}");

            UnhandledMessages.Add(builder.ToString());
        }
    }
}