using System;
using System.Text;
using Serilog;
using Serilog.Events;
using Serilog.Parsing;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class SetupServer : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<SetupServer>();
        private readonly Action<string> _log;

        public void LogMessage(string template, params object[] args)
        {
            Logger.Information(template, args);

            var parser = new MessageTemplateParser();
            var template2 = parser.Parse(template);
            var format = new StringBuilder();
            var index = 0;
            foreach (var tok in template2.Tokens)
            {
                if (tok is TextToken)
                    format.Append(tok);
                else
                    format.Append("{" + index++ + "}");
            }
            var netStyle = format.ToString();

            _log(string.Format(netStyle, args));
        }

        public SetupServer(Action<string> log) 
            => _log = log;

        public void AddPendingInstallations(string id, string location)
        {

        }

        public void Dispose()
        {
        }
    }
}