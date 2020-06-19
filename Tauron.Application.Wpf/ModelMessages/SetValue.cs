namespace Tauron.Application.Wpf.ModelMessages
{
    public sealed class SetValue
    {
        public string Name { get; }

        public object? Value { get; }

        public SetValue(string name, object? value)
        {
            Name = name;
            Value = value;
        }

        public void Deconstruct(out string name, out object? value)
        {
            name = Name;
            value = Value;
        }
    }
}