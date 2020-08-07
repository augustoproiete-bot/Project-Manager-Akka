using System;
using System.Collections.Immutable;

namespace SimpleHostSetup.Impl
{
    public sealed class BuildSystemConfiguration
    {
        public string SearchStart { get; }

        public string SearchRootFile { get; }

        public IInput Input { get; }

        public Func<IApplicationBuilder> BuilderFactory { get; }

        public ImmutableDictionary<string, AppInfo> AppMapping { get; }

        public string HostProject { get; }

        public BuildSystemConfiguration(string searchStart, string searchRootFile, IInput input, Func<IApplicationBuilder> builderFactory, ImmutableDictionary<string, AppInfo> appMapping, 
            string hostProject)
        {
            SearchStart = searchStart;
            SearchRootFile = searchRootFile;
            Input = input;
            BuilderFactory = builderFactory;
            AppMapping = appMapping;
            HostProject = hostProject;
        }
    }
}