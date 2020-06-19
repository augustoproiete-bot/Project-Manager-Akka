namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class ValidatingEvent
    {
        public string? Reason { get; }

        public string Name { get; }

        public bool Error => !string.IsNullOrWhiteSpace(Reason);

        public ValidatingEvent(string? reason, string name)
        {
            Reason = reason;
            Name = name;
        }
    }
}