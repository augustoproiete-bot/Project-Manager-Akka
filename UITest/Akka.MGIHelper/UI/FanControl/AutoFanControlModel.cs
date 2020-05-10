using System.Windows.Threading;
using Akka.Actor;
using Akka.MGIHelper.Core.Configuration;
using Akka.MGIHelper.Core.FanControl;
using Akka.MGIHelper.Core.FanControl.Events;
using Autofac;
using Tauron.Application.Wpf.Model;

namespace Akka.MGIHelper.UI.FanControl
{
    public class AutoFanControlModel : UiActor
    {
        private bool Error
        {
            set => Set(value);
        }

        private string Reason
        {
            set => Set(value);
        }

        private int Power
        {
            set => Set(value);
        }

        private State State
        {
            set => Set(value);
        }

        private double Pidout
        {
            set => Set(value);
        }

        private int PidSetValue
        {
            set => Set(value);
        }

        private int Pt1000
        {
            set => Set(value);
        }

        private bool FanRunning
        {
            set => Set(value);
        }

        private FanControlOptions Options
        {
            set => Set(value);
        }

        public AutoFanControlModel(ILifetimeScope scope, Dispatcher dispatcher, FanControlOptions options)
            : base(scope, dispatcher)
        {
            Options = options;
            Context.ActorOf(Props.Create(() => new Core.FanControl.FanControl(options)), "Fan-Control");

            FanRunning = false;

            Receive<TrackingEvent>(evt =>
                                   {
                                       Error = evt.Error;
                                       Reason = evt.Reason;
                                       Power = evt.Power;
                                       State = evt.State;
                                       Pidout = evt.Pidout;
                                       PidSetValue = evt.PidSetValue;
                                       Pt1000 = evt.Pt1000;
                                   });
            Receive<FanStatusChange>(evt => FanRunning = evt.Running);
        }

        protected override void Initialize() 
            => Context.Child("Fan-Control").Tell(new ClockEvent(ClockState.Start));
    }
}