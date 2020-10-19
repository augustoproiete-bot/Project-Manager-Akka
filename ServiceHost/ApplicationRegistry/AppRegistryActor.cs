using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiceHost.Services;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    [UsedImplicitly]
    public sealed class AppRegistryActor : ExposedReceiveActor
    {
        private const string BaseFileName = "apps.dat";
        private const string AppFileExt = ".app";

        private readonly Dictionary<string, string> _apps = new Dictionary<string, string>();
        private readonly string _appsDirectory;
        private readonly SubscribeAbility _subscribeAbility;

        private readonly Dictionary<Guid, IActorRef> _ongoingQuerys = new Dictionary<Guid, IActorRef>();
        private readonly IAppManager _appManager;

        public AppRegistryActor(IConfiguration configuration, IAppManager appManager)
        {
            _appManager = appManager;
            _subscribeAbility = new SubscribeAbility(this);
            _appsDirectory = Path.GetFullPath(configuration["AppsLocation"]);

            Flow<AllAppsQuery>(b => b.Func(_ => new AllAppsResponse(_apps.Keys.ToArray())).ToSender());

            Receive<LoadData>(HandleLoadData);
            Receive<SaveData>(HandleSaveData);
            
            Receive<InstalledAppQuery>(HandleQueryApp);
            Receive<NewRegistrationRequest>(HandleNewRegistration);
            Receive<UpdateRegistrationRequest>(HandleUpdateRequest);

            Receive<QueryHostApps>(ProcessQuery);
            Receive<AppStatusResponse>(FinishQuery);

            _subscribeAbility.MakeReceive();
        }

        #region SharedApi

        private void FinishQuery(AppStatusResponse msg)
        {
            if (_ongoingQuerys.Remove(msg.OpId, out var sender))
            {
                var apps = System.Collections.Immutable.ImmutableList<HostApp>.Empty.ToBuilder();

                foreach (var appsKey in _apps.Keys)
                {
                    try
                    {
                        var app = LoadApp(appsKey);
                        if(app == null) continue;

                        apps.Add(new HostApp(app.Name, app.Path, app.Version, app.AppType, app.SuressWindow, app.Exe, msg.Apps.GetValueOrDefault(appsKey, false)));
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Error on Load Installed Apps");
                    }
                }

                sender.Tell(new HostAppsResponse(apps.ToImmutable()));
            }
        }

        private void ProcessQuery(QueryHostApps obj)
        {
            var id = Guid.NewGuid();
            _ongoingQuerys[id] = Sender;
            _appManager.Actor.Tell(new QueryAppStaus(id));
        }

        #endregion

        private void HandleUpdateRequest(UpdateRegistrationRequest request)
        {
            RegistrationResponse response;
            Log.Info("Update Registraion for {Apps}", request.Name);

            try
            {
                if (!_apps.TryGetValue(request.Name, out var path))
                {
                    Log.Warning("No Registration Found {Apps}", request.Name);
                    response = new RegistrationResponse(true, null);
                }
                else
                {
                    var newData = JsonConvert.DeserializeObject<InstalledApp>(File.ReadAllText(path)).NewVersion();
                    File.WriteAllText(path, JsonConvert.SerializeObject(newData));
                    response = new RegistrationResponse(true, null);

                    Log.Info("Registration Update Compled {Apps}", request.Name);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while Reading or Writing Registration {Apps}", request.Name);
                response = new RegistrationResponse(false, e);
            }

            Sender.Tell(response);
            _subscribeAbility.Send(response);
        }

        private void HandleNewRegistration(NewRegistrationRequest request)
        {
            Log.Info("Register new Application {Apps}", request.Name);
            RegistrationResponse response;

            try
            {
                if (_apps.ContainsKey(request.Name))
                {
                    Log.Warning("Attempt to Register Duplicate Application {Apps}", request.Name);
                    response = new RegistrationResponse(false, new InvalidOperationException("Duplicate"));
                }
                else
                {
                    string fullPath = Path.GetFullPath(request.Path + AppFileExt);
                    File.WriteAllText(fullPath, 
                        JsonConvert.SerializeObject(new InstalledApp(request.Name, request.Path, request.Version, request.AppType, request.SupressWindow, request.ExeFile)));
                    _apps[request.Name] = fullPath;

                    response = new RegistrationResponse(true, null);
                    Self.Tell(new SaveData());

                    Log.Info("Registration Compled for {Apps}", request.Name);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while registration new Application {Apps}", request.Name);
                response = new RegistrationResponse(false, e);
            }

            Sender.Tell(response);
            _subscribeAbility.Send(response);
        }

        private void HandleQueryApp(InstalledAppQuery request)
        {
            Log.Info("Query Apps {Apps}", request.Name);
            try
            {
                Sender.Tell(new InstalledAppRespond(LoadApp(request.Name) ?? InstalledApp.Empty));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error While Querining Apps Data");
                Sender.Tell(new InstalledAppRespond(InstalledApp.Empty) { Fault = true } );
            }
        }

        private InstalledApp? LoadApp(string name)
        {
            if (_apps.TryGetValue(name, out var path) && File.Exists(path))
            {
                var data = JsonConvert.DeserializeObject<InstalledApp>(path.ReadTextIfExis());
                Log.Info("Load Apps Data Compled {Apps}", name);
                return data;
            }

            Log.Info("No Apps Found {Apps}", name);
            return null;
        }

        private void HandleSaveData(SaveData unused)
        {
            try
            {
                string file = Path.Combine(_appsDirectory, BaseFileName);
                using (var fileStream = new StreamWriter(File.Open(file, FileMode.Create)))
                {
                    foreach (var (name, path) in _apps)
                        fileStream.WriteLine($"{name}:{path}");
                }

                File.Copy(file, file + ".bak", true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while writing Apps Info");
            }
        }

        private void HandleLoadData(LoadData unused)
        {
            _apps.Clear();
            string file = Path.Combine(_appsDirectory, BaseFileName);
            if(!File.Exists(file))
                return;

            try
            {
                TryRead(file);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Error while Loading AppInfos");

                try
                {
                    _apps.Clear();
                    TryRead(file + ".bak");
                }
                catch (Exception exception)
                {
                    Log.Error(exception, "Error while Loading AppInfos backup");
                }
            }
        }

        private void TryRead(string file)
        {
            foreach (var line in File.ReadAllLines(file))
            {
                var split = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);

                _apps[split[0].Trim()] = split[1].Trim();

            }
        }

        protected override void PreStart()
        {
            Self.Tell(new LoadData());
            base.PreStart();
        }

        private sealed class LoadData { }
        private sealed class SaveData { }
    }
}