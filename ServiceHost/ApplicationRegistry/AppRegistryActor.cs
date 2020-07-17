using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Tauron;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services.Core;

namespace ServiceHost.ApplicationRegistry
{
    [UsedImplicitly]
    public sealed class AppRegistryActor : ExposedReceiveActor
    {
        private const string BaseFileName = "apps.data";
        private const string AppFileExt = ".app";

        private readonly Dictionary<string, string> _apps = new Dictionary<string, string>();
        private readonly string _appsDirectory;
        private readonly SubscribeAbility _subscribeAbility;

        public AppRegistryActor(IConfiguration configuration)
        {
            _subscribeAbility = new SubscribeAbility(this);
            _appsDirectory = Path.GetFullPath(configuration["AppsLocation"]);

            Receive<LoadData>(HandleLoadData);
            Receive<SaveData>(HandleSaveData);
            
            Receive<InstalledAppQuery>(HandleQueryApp);
            Receive<NewRegistrationRequest>(HandleNewRegistration);
            Receive<UpdateRegistrationRequest>(HandleUpdateRequest);

            _subscribeAbility.MakeReceive();
        }

        private void HandleUpdateRequest(UpdateRegistrationRequest request)
        {
            RegistrationResponse response;
            
            try
            {
                if (!_apps.TryGetValue(request.Name, out var path))
                {
                    response = new RegistrationResponse(true, null);
                }
                else
                {
                    var newData = JsonConvert.DeserializeObject<InstalledApp>(File.ReadAllText(path)).NewVersion();
                    File.WriteAllText(path, JsonConvert.SerializeObject(newData));
                    response = new RegistrationResponse(true, null);
                }
            }
            catch (Exception e)
            {
                response = new RegistrationResponse(false, e);
            }

            Sender.Tell(response);
            _subscribeAbility.Send(response);
        }

        private void HandleNewRegistration(NewRegistrationRequest request)
        {
            RegistrationResponse response;

            try
            {
                if(_apps.ContainsKey(request.Name))
                    response = new RegistrationResponse(false, new InvalidOperationException("Duplicate"));
                else
                {
                    string fullPath = Path.GetFullPath(request.Path + AppFileExt);
                    File.WriteAllText(fullPath, JsonConvert.SerializeObject(new InstalledApp(request.Name, request.Path, request.Version, request.AppType, request.SupressWindow)));

                    response = new RegistrationResponse(true, null);
                    Self.Tell(new SaveData());
                }
            }
            catch (Exception e)
            {
                response = new RegistrationResponse(false, e);
            }

            Sender.Tell(response);
            _subscribeAbility.Send(response);
        }

        private void HandleQueryApp(InstalledAppQuery request)
        {
            try
            {
                if (_apps.TryGetValue(request.Name, out var path) && File.Exists(path))
                {
                    var data = JsonConvert.DeserializeObject<InstalledApp>(path.ReadTextIfExis());
                    Sender.Tell(new InstalledAppRespond(data));
                }
                else
                    Sender.Tell(new InstalledAppRespond(InstalledApp.Empty));
            }
            catch (Exception e)
            {
                Log.Error(e, "Error While Querining App Data");
                Sender.Tell(new InstalledAppRespond(InstalledApp.Empty) { Fault = true } );
            }
        }

        private void HandleSaveData(SaveData unused)
        {
            try
            {
                string file = Path.Combine(_appsDirectory, BaseFileName);
                using var fileStream = new StreamWriter(File.Open(file, FileMode.Create));

                foreach (var (name, path) in _apps) 
                    fileStream.WriteLine($"{name}:{path}");

                File.Copy(file, file + ".bak", true);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error while writing App Info");
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