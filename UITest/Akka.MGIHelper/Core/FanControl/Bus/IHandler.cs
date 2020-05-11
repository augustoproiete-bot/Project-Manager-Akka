using System.Threading.Tasks;

namespace Akka.MGIHelper.Core.FanControl.Bus
{
    public interface IHandler<in TMessage>
    {
        Task Handle(TMessage msg, MessageBus messageBus);
    }
}