using System.Globalization;
using Akka.MGIHelper.Settings;
using Tauron.Akka;

namespace Akka.MGIHelper.Core.Configuration
{
    public sealed class FanControlOptions : ConfigurationBase
    {
        public FanControlOptions(IDefaultActorRef<SettingsManager> actor, string scope) : base(actor, scope)
        {
        }

        public int ClockTimeMs
        {
            get => GetValue(int.Parse, 1000)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public string Ip
        {
            get => GetValue(s => s, "192.168.187.48")!;
            set => SetValue(value);
        }

        public int GoStandbyTime
        {
            get => GetValue(int.Parse, 25)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStartupTemp
        {
            get => GetValue(int.Parse, 70)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public int MaxStandbyTemp
        {
            get => GetValue(int.Parse, 115)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }

        public double FanControlMultipler
        {
            get => GetValue(double.Parse, 1.3d)!;
            set => SetValue(value.ToString(CultureInfo.InvariantCulture));
        }
    }
}