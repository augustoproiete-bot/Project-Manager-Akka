namespace Akka.Code.Configuration.Converter
{
    public sealed class ArrayConverter : ConverterBase
    {
        public static ArrayConverter Instance = new ArrayConverter();

        public override string? ToElementValue(object? obj)
        {
            return obj?.ToString();
        }
    }
}