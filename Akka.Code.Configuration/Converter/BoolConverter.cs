namespace Akka.Code.Configuration.Converter
{
    public class BoolConverter : TypedConverter<bool>
    {
        public static readonly BoolConverter Instance = new BoolConverter();
        protected override string? ConvertGeneric(bool obj) => obj.ToString().ToLower();
    }
}