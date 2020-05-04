namespace Akka.Code.Configuration.Converter
{
    public sealed class GenericConverter : ConverterBase
    {
        public static readonly GenericConverter Converter = new GenericConverter();

        public override string? ToElementValue(object? obj)
        {
            var temp = obj?.ToString();

            return temp?.Contains('/') == true ? $"\"{temp}\"" : temp;
        }
    }
}