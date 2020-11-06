using Tauron.Application.Workshop.StateManagement;

namespace Tauron.Application.ServiceManager.Core.Managment.Events
{
    public sealed class AddSeedUrlAction : SimpleStateAction
    {
        public string Url { get; }

        public AddSeedUrlAction(string url) => Url = url;
    }
}