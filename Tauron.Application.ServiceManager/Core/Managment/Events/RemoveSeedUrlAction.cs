using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class RemoveSeedUrlAction : SimpleStateAction
    {
        public string Url { get; }

        public RemoveSeedUrlAction(string url) => Url = url;
    }
}