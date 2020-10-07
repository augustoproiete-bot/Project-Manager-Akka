using System;
using Tauron.Application.Master.Commands.Administration.Host;

namespace ServiceHost.ApplicationRegistry
{
    public sealed class NewRegistrationRequest
    {
        public string Name { get; }

        public string Path { get; }

        public int Version { get; }

        public AppType AppType { get; }

        public bool SupressWindow { get; }

        public string ExeFile { get; }

        public NewRegistrationRequest(string name, string path, int version, AppType appType, string exeFile)
        {
            Name = name;
            Path = path;
            Version = version;
            AppType = appType;
            ExeFile = exeFile;
        }
    }

    public sealed class RegistrationResponse
    {
        public bool Scceeded { get; }

        public Exception? Error { get; }

        public RegistrationResponse(bool scceeded, Exception? error)
        {
            Scceeded = scceeded;
            Error = error;
        }
    }
}