using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Configuration;
using Serilog;
using Serilog.Parsing;
using Servicemnager.Networking;
using Servicemnager.Networking.Server;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class SetupServer : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<SetupServer>();

        private static int _installerCount = 0;

        private readonly ConcurrentDictionary<string, InstallerOperation> _pendingOperations = new ConcurrentDictionary<string, InstallerOperation>();

        private readonly Lazy<DataServer> _dataServer;

        private readonly Action<string> _log;

        private void LogMessage(string template, params object[] args)
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

        public SetupServer(Action<string> log, Config settings)
        {
            _log = log;

            SetupBuilder.BuildRoot.DeleteDirectory(true);

            _dataServer = new Lazy<DataServer>(() =>
            {
                var serv = new DataServer(settings.GetString("akka.remote.dot-netty.tcp.hostname"));
                
                serv.OnMessageReceived += ServOnOnMessageReceived;

                return serv;
            });
        }
        
        private void ServOnOnMessageReceived(object? sender, MessageFromClientEventArgs e)
        {
            try
            {
                switch (e.Message.Type)
                {
                    case NetworkOperation.Identifer:
                        var id = Encoding.UTF8.GetString(e.Message.Data);
                        LogMessage("Recived Id {Id}", id);

                        if (_pendingOperations.TryGetValue(id, out var op))
                        {
                            LogMessage("Id Accepted {Id}", id);
                            op.EndpointId = e.Client;
                            Task.Run(() =>
                            {
                                _dataServer.Value.Send(e.Client, NetworkMessage.Create(NetworkOperation.Accept, Array.Empty<byte>()));
                                TransmitData(id, op);
                            });
                        }
                        else
                        {
                            LogMessage("No Operation Found {Id}", id);
                            SendDeny(e.Client);
                        }

                        break;
                }
            }
            catch (Exception exception)
            {
                LogMessage("Error on Processing Message {Error}", exception.Message);
                SendDeny(e.Client);
            }
        }

        private void SendDeny(string client) 
            => Task.Run(() => _dataServer.Value.Send(client, NetworkMessage.Create(NetworkOperation.Deny, Array.Empty<byte>())));

        private void TransmitData(string id, InstallerOperation operation)
        {
            try
            {
                using var file = operation.Builder(s => _dataServer.Value.Send(operation.EndpointId, NetworkMessage.Create(NetworkOperation.Message, Encoding.UTF8.GetBytes(s))), 
                    id, operation.EndpointId);
                if (file == null || string.IsNullOrWhiteSpace(file.Zip))
                    SendDeny(operation.EndpointId);

                LogMessage("Transmiting Data {Id}", id);

                using var dataStream = File.OpenRead(file.Zip);
                var buffer = ArrayPool<byte>.Shared.Rent(50000);

                while (true)
                {
                    var count = dataStream.Read(buffer, 0, buffer.Length);
                    _dataServer.Value.Send(operation.EndpointId, NetworkMessage.Create(NetworkOperation.Data, buffer, count));

                    if (dataStream.Position != dataStream.Length) continue;

                    _dataServer.Value.Send(operation.EndpointId, NetworkMessage.Create(NetworkOperation.Compled, Array.Empty<byte>()));
                    break;
                }

                file.Compled();
            }
            catch (Exception e)
            {
                LogMessage("Error on Transmiting Data {Id} {Error}", id, e.Message);
                SendDeny(operation.EndpointId);
            }
        }

        public void AddPendingInstallations(string id, Func<Action<string>, string, string, BuildResult?> builder)
        {
            if(!_dataServer.IsValueCreated)
                _dataServer.Value.Start();

            if (!_pendingOperations.TryAdd(id, new InstallerOperation(builder, () => _pendingOperations.TryRemove(id, out _)))) 
                throw new InvalidOperationException("Adding to Pending Operations Failed");
            
            var number = Interlocked.Increment(ref _installerCount);
            var installerFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"HostInstaller{number}.zip");
            if (File.Exists(installerFile))
                File.Delete(installerFile);

            File.Copy(Path.GetFullPath("HostInstaller\\Installer.zip"), installerFile);
            HostConfiguration.WriteInTo(installerFile, _dataServer.Value.EndPoint.ToString(), id);

            Process.Start("explorer.exe", Path.GetDirectoryName(installerFile));
        }

        public void Dispose()
        {
            if(_dataServer.IsValueCreated)
                _dataServer.Value.Dispose();

            foreach (var operation in _pendingOperations.Values) 
                operation.Dispose();
        }

        private sealed class InstallerOperation : IDisposable
        {
            public Func<Action<string>, string, string, BuildResult?> Builder { get; }

            public string EndpointId { get; set; } = string.Empty;

            private Timer Timeout { get; }

            public InstallerOperation(Func<Action<string>, string, string, BuildResult?> builder, Action remove)
            {
                Builder = builder;
                Timeout = new Timer(_ => remove(), null, TimeSpan.FromMinutes(30), System.Threading.Timeout.InfiniteTimeSpan);
            }

            public void Dispose() => Timeout.Dispose();
        }
    }
}