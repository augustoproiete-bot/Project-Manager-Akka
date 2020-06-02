using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Akka.MGIHelper.Core.FanControl.Events;

namespace Akka.MGIHelper.UI.FanControl
{
    [ValueConversion(typeof(State), typeof(string))]
    public class LampStatusConverter : IValueConverter
    {
        private static readonly Dictionary<State, string> StadesLabels = new Dictionary<State, string>
        {
            {State.Cooldown, "Abkühlen"},
            {State.Error, "Fehler"},
            {State.Idle, "Aus"},
            {State.Ignition, "Zünden"},
            {State.Ready, "Bereit"},
            {State.Power, "Betrieb"},
            {State.StandBy, "Bereitschaft"},
            {State.StartUp, "Starten"},
            {State.TestRun, "Test"}
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Keine Daten";
            return StadesLabels[(State) value];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return State.Idle;
        }
    }
}