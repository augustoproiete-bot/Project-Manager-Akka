using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectSaver : ReceiveActor, IWithTimers
    {
        private readonly List<(SaveProject toSave, IActorRef Sender)> _toSave = new List<(SaveProject toSave, IActorRef Sender)>();
        private bool _sealed;

        public ProjectSaver()
        {
            Receive<SaveProject>(SaveProject);
            Receive<InitSave>(StartNormalSave);
            Receive<ForceSave>(TryForceSave);

            Timers.StartSingleTimer(nameof(InitSave), new InitSave(), TimeSpan.FromSeconds(1));
        }

        public ITimerScheduler Timers { get; set; } = null!;

        private void TryForceSave(ForceSave obj)
        {
            Timers.Cancel(nameof(InitSave));
            StartNormalSave(null);

            TrySave(obj.File);
            if (!obj.AndSeal) return;

            _sealed = true;
            Timers.CancelAll();
        }

        private void StartNormalSave(InitSave? obj)
        {
            if (_toSave.Count > 1)
                foreach (var (toSave, sender) in _toSave.Take(_toSave.Count - 1))
                    sender.Tell(new SavedProject(toSave.OperationId, true, null));

            if (_toSave.Count > 0)
            {
                var (saveProject, send) = _toSave[^1];
                var result = TrySave(saveProject.ProjectFile);
                send.Tell(new SavedProject(saveProject.OperationId, result == null, result));
            }

            Timers.Cancel(nameof(InitSave));
            Timers.StartSingleTimer(nameof(InitSave), new InitSave(), TimeSpan.FromSeconds(1));
        }

        private void SaveProject(SaveProject obj)
        {
            if (_sealed) return;

            _toSave.Add((obj, Sender));
        }

        private static Exception? TrySave(ProjectFile file)
        {
            try
            {
                if (file.Source.ExisFile())
                    File.Copy(file.Source, file.Source + ".bak", true);
                using var stream = File.Open(file.Source, FileMode.Create);
                using var writer = new BinaryWriter(stream);
                file.Write(writer);

                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        private sealed class InitSave
        {
        }
    }
}