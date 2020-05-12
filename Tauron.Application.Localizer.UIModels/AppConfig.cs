using System;
using System.Collections.Immutable;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.UIModels.Settings;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class AppConfig : ConfigurationBase
    {
        private ImmutableList<string>? _renctFiles;

        public ImmutableList<string> RenctFiles
        {
            get
            {
                if(_renctFiles == null)
                    _renctFiles = ImmutableList<string>.Empty.AddRange(GetValue(s => s.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries), Array.Empty<string>()));
                return _renctFiles;
            }
            set
            {
                if(_renctFiles == null)
                    throw new InvalidOperationException("Renct Files Must be set before used");
                if (_renctFiles.Count > 10)
                    _renctFiles = _renctFiles.RemoveAt(_renctFiles.Count - 1);
            }
        }

        public AppConfig(IDefaultActorRef<SettingsManager> actor, string scope) : base(actor, scope)
        {
        }
    }
}