using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Akka.Configuration;
using Serilog;
using Serilog.Parsing;
using Servicemnager.Networking;
using Servicemnager.Networking.Server;
using Servicemnager.Networking.Transmitter;

namespace Tauron.Application.ServiceManager.Core.SetupBuilder
{
    public sealed class SetupServer : IDisposable
    {
        private static readonly ILogger Logger = Log.ForContext<SetupServer>();

        private static int _installerCount;

        private readonly ConcurrentDictionary<string, InstallerOperation> _pendingOperations = new ConcurrentDictionary<string, InstallerOperation>();

        private readonly ConcurrentDictionary<string, InstallerOperation> _runningOperations = new ConcurrentDictionary<string, InstallerOperation>();

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

            _dataServer = new Lazy<DataServer>(() =>
            {
                var serv = new DataServer(settings.GetString("akka.remote.dot-netty.tcp.hostname"));
                
                serv.OnMessageReceived += ServOnOnMessageReceived;

                return serv;
            });
        }
        
        private async void ServOnOnMessageReceived(object? sender, MessageFromClientEventArgs e)
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


                            LogMessage("Bind {Id} To {Client}", id, e.Client);
                            if (!_runningOperations.TryAdd(e.Client, op))
                            {
                                SendDeny(e.Client);
                                op.Dispose();
                            }
                            else
                            {
                                var result = await BuildData(id, op);
                                if (result == null)
                                {
                                    SendDeny(e.Client);
                                    op.Dispose();
                                }
                                else
                                {
                                    LogMessage("Sending Data {Id}", id);
                                    var pool = ArrayPool<byte>.Shared;
                                    op.Sender = new Sender(result.Zip.Stream, e.Client, _dataServer.Value,
                                        () => pool.Rent(50_000), bytes => pool.Return(bytes, true),
                                        exception => LogMessage("Error on Processing Message \"{Message}\" {Id}", exception.Message, id));

                                    op.Result = result;
                                    op.Sender.ProcessMessage(e.Message);
                                }
                            }

                            _pendingOperations.TryRemove(id, out _);
                        }
                        else
                        {
                            LogMessage("No Operation Found {Id}", id);
                            SendDeny(e.Client);
                        }
                        break;
                    default:
                        if (_runningOperations.TryGetValue(e.Client, out var running) && running.Sender != null && running.Result != null)
                        {
                            if (!running.Sender.ProcessMessage(e.Message))
                            {
                                LogMessage("Transmission Compled {Client}", e.Client);
                                running.Result.Compled();
                                running.Dispose();
                            }
                        }
                        else
                            SendDeny(e.Client);
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
        {
            LogMessage("Communication Error {Client}", client);
            Task.Run(() => _dataServer.Value.Send(client, NetworkMessage.Create(NetworkOperation.Deny, Array.Empty<byte>())));
        }

        private async Task<BuildResult?> BuildData(string id, InstallerOperation operation)
        {
            try
            {
                var file = await operation.BuilderFunc(s => _dataServer.Value.Send(operation.EndpointId, NetworkMessage.Create(NetworkOperation.Message, Encoding.UTF8.GetBytes(s))),
                    id, operation.EndpointId);
                if (file == null)
                    SendDeny(operation.EndpointId);

                return file;
            }
            catch (Exception e)
            {
                LogMessage("Error on Transmiting Data {Id} {Error}", id, e.Message);
                SendDeny(operation.EndpointId);
            }

            return null;
        }

        public void AddPendingInstallations(string id, SetupBuilder builder, bool addShortcurt)
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
            HostConfiguration.WriteInTo(installerFile, _dataServer.Value.EndPoint.ToString()!, id, addShortcurt);

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
            private readonly SetupBuilder _builder;
            public Func<Action<string>, string, string, Task<BuildResult?>> BuilderFunc { get; }

            public string EndpointId { get; set; } = string.Empty;

            private Timer Timeout { get; }

            public Sender? Sender { get; set; }

            public BuildResult? Result { get; set; }

            public InstallerOperation(SetupBuilder builder, Action remove)
            {
                _builder = builder;
                BuilderFunc = builder.Run;
                Timeout = new Timer(_ =>
                {
                    Sender?.Dispose();
                    Sender = null;
                    Result?.Dispose();
                    Result = null;
                    remove();
                }, null, TimeSpan.FromMinutes(30), System.Threading.Timeout.InfiniteTimeSpan);
            }

            public void Dispose()
            {
                _builder.Dispose();
                Timeout.Dispose();
                Result?.Dispose();
            }
        }
    }
}