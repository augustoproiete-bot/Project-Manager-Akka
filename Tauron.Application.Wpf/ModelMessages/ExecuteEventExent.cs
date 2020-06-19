using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class ExecuteEventExent
    {
        public EventData Data { get; }

        public string Name { get; }

        public ExecuteEventExent(EventData data, string name)
        {
            Data = data;
            Name = name;
        }

        public void Deconstruct(out EventData eventdata, out string name)
        {
            eventdata = Data;
            name = Name;
        }
    }
}