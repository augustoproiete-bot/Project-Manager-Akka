using System;
using System.Collections.Generic;
using System.IO;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectSaver : ReceiveActor, IWithTimers
    {
        private sealed class InitSave
        {
            
        }

        private readonly List<SaveProject> _toSave = new List<SaveProject>();

        public ProjectSaver()
        {
            Receive<SaveProject>(SaveProject);
            Timers.CancelAll();
        }

        private void SaveProject(SaveProject obj)
        {
            try
            {
                if(obj.ProjectFile.Source.ExisFile())
                    File.Copy(obj.ProjectFile.Source, obj.ProjectFile.Source + ".bak", true);
                using var stream = File.Open(obj.ProjectFile.Source, FileMode.Create);
                using var writer = new BinaryWriter(stream);
                obj.ProjectFile.Write(writer);

                Sender.Tell(new SavedProject(obj.OperationId, true, null));
            }
            catch (Exception e)
            {
                Sender.Tell(new SavedProject(obj.OperationId, false, e));
            }
        }

        public ITimerScheduler Timers { get; set; } = null!;
    }
}