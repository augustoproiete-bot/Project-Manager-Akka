using System.Threading.Tasks;

namespace MGIHelper.Core.Bus
{
    public interface IHandler<in TMessage>
    {
        Task Handle(TMessage msg, MessageBus messageBus);
    }
}