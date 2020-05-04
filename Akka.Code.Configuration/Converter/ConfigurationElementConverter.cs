using System.Text;

namespace Akka.Code.Configuration.Converter
{
    public sealed class ConfigurationElementConverter : TypedConverter<ConfigurationElement>
    {
        public static readonly ConfigurationElementConverter Instance = new ConfigurationElementConverter();

        protected override string? ConvertGeneric(ConfigurationElement obj)
        {
            var builder = new StringBuilder();
            builder.AppendLine("{");
            obj.Construct(builder);
            builder.AppendLine("}");

            return builder.ToString();
        }
    }
}