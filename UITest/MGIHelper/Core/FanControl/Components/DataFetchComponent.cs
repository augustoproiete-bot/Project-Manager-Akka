using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MGIHelper.Core.Bus;
using MGIHelper.Core.Configuration;
using MGIHelper.Core.FanControl.Events;

namespace MGIHelper.Core.FanControl.Components
{
    public sealed class DataFetchComponent : IHandler<TickEvent>, IDisposable
    {
        private static readonly Dictionary<string, State> StatesMapping = new Dictionary<string, State>
        {
            { "Idle".ToLower(), State.Idle},
            { "Ready".ToLower(), State.Ready },
            { "Ignition".ToLower(), State.Ignition },
            { "Start_up".ToLower(), State.StartUp },
            { "Stand_by".ToLower(), State.StandBy },
            { "Power".ToLower(), State.Power },
            { "Cool_down".ToLower(), State.Cooldown },
            { "Test_run".ToLower(), State.TestRun },
            { "Error".ToLower(), State.Error }
        };

        private readonly WebClient _webClient = new MyWebClient();
        private readonly FanControlOptions _options;

        public DataFetchComponent(FanControlOptions options) => _options = options;

        public async Task Handle(TickEvent msg, MessageBus messageBus)
        {
            try
            {
                var trackingString = _webClient.DownloadString($"http://{_options.Ip}/html/top.html?SysStatusData?");

                var elements = trackingString.Split("&");

                var pairs = elements.Select(s => s.Split("=")).Select(ele => new ValuePair { Name = ele[0], Value = ele[1] }).ToArray();

                var power = int.Parse(pairs.First(p => p.Name.ToLower() == "power").Value);
                var state = StatesMapping[pairs.First(p => p.Name.ToLower() == "sysstate").Value.ToLower()];
                var pidout = double.Parse(pairs.First(p => p.Name.ToLower() == "pidout").Value) / 10;
                var pidSetValue = int.Parse(pairs.First(p => p.Name.ToLower() == "pidsetvalue").Value);
                var pt1000 = int.Parse(pairs.First(p => p.Name.ToLower() == "pt1000").Value);

                await messageBus.Publish(new TrackingEvent(power, state, pidout, pidSetValue, pt1000));
            }
            catch (Exception e)
            {
                await messageBus.Publish(new TrackingEvent(true, e.Message));
            }
        }

        public void Dispose() => _webClient?.Dispose();

        private class ValuePair
        {
            public string Name { get; set; }

            public string Value { get; set; }

            public override string ToString()
            {
                return $"{Name}={Value}";
            }
        }

        private class MyWebClient : WebClient
        {
            protected override WebRequest GetWebRequest(Uri uri)
            {
                var w = base.GetWebRequest(uri);
                if(w != null)
                    w.Timeout = 5000;
                return w;
            }
        }
    }
}