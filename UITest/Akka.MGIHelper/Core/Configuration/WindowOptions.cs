using System.Globalization;
using Akka.MGIHelper.Settings;
using Tauron.Akka;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class WindowOptions : ConfigurationBase
    {
        public WindowOptions(IDefaultActorRef<SettingsManager> actor, string scope)
            : base(actor, scope)
        {
        }

        public double PositionX
        {
            get => GetValue(double.Parse)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double PositionY
        {
            get => GetValue(double.Parse)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}