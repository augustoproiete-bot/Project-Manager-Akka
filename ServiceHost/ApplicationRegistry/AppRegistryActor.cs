using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Tauron;
using Tauron.Akka;

namespace ServiceHost.ApplicationRegistry
{
    [UsedImplicitly]
    public sealed class AppRegistryActor : ExposedReceiveActor
    {
        private const string BaseFileName = "apps.data";
        private const string AppFileExt = ".app";

        private readonly Dictionary<string, string> _apps = new Dictionary<string, string>();
        private readonly string _appsDirectory;

        public AppRegistryActor(IConfiguration configuration)
        {
            _appsDirectory = Path.GetFullPath(configuration["AppsLocation"]);

            Receive<LoadData>(HandleLoadData);
            Receive<SaveData>(HandleSaveData);
            
            Receive<InstalledAppQuery>(HandleQueryApp);
            Receive<NewRegistrationRequest>(HandleNewRegistration);
            Receive<UpdateRegistrationRequest>(HandleUpdateRequest);
        }

        private void HandleUpdateRequest(UpdateRegistrationRequest request)
        {
            try
            {
                if (!_apps.TryGetValue(request.Name, out var path))
                {
                    Sender.Tell(new RegistrationResponse(true, null));
                }
                else
                {
                    var newData = JsonConvert.DeserializeObject<InstalledApp>(File.ReadAllText(path)).NewVersion();
                    File.WriteAllText(path, JsonConvert.SerializeObject(newData));
                }
            }
            catch (Exception e)
            {
                Sender.Tell(new RegistrationResponse(false, e));
            }
        }

        private void HandleNewRegistration(NewRegistrationRequest request)
        {
            try
            {
                if(_apps.ContainsKey(request.Name))
                    Sender.Tell(new RegistrationResponse(false, new InvalidOperationException("Duplicate")));
                else
                {
                    string fullPath = Path.GetFullPath(request.Path + AppFileExt);
                    File.WriteAllText(fullPath, JsonConvert.SerializeObject(new InstalledApp(request.Name, request.Path, request.Version, request.AppType, request.SupressWindow)));

                    Sender.Tell(new RegistrationResponse(true, null));
                    Self.Tell(new SaveData());
                }
            }
            catch (Exception e)
            {
                Sender.Tell(new RegistrationResponse(false, e));
            }
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