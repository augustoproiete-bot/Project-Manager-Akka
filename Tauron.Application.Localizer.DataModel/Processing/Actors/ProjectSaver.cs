using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Actor;
using Functional.Either;
using Functional.Maybe;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectSaver : StatefulReceiveActor<ProjectSaver.ProjectSaverState>, IWithTimers
    {
        public sealed record ProjectSaverState(ImmutableList<(SaveProject ToSave, IActorRef Sender)> ToSave, bool IsSealed);

        public ProjectSaver()
            : base(new ProjectSaverState(ImmutableList<(SaveProject ToSave, IActorRef Sender)>.Empty, false))
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

            TrySave(May(obj.File));
            if (!obj.AndSeal) return;

            Run(s => from state in s
                     select state with{IsSealed = true});
            Timers.CancelAll();
        }

        private void StartNormalSave(InitSave? obj)
        {
            Maybe<Unit> SendBack(ImmutableList<(SaveProject ToSave, IActorRef Sender)> tosave, Either<Unit, Exception> result)
            {
                var isOk = Match(result,
                                 _ => true,
                                 e =>
                                 {
                                     Log.Warning(e, "Error on Save Project");
                                     return false;
                                 });
                
                var error = MayNotNull(result.ErrorOrDefault());
                
                foreach (var save in tosave)
                {
                    var (proj, sender) = save;
                    sender.Tell(new SavedProject(isOk, error, proj.OperationId));
                }
                
                return Unit.MayInstance;
            }

            Run(s => from state in s
                     where state.ToSave.Count > 0
                     let project = May(state.ToSave[^1].ToSave.ProjectFile)
                     let result = TrySave(project)
                     from _ in SendBack(state.ToSave, result)
                     select state with{ToSave = ImmutableList<(SaveProject ToSave, IActorRef Sender)>.Empty});

            Do(from cancel in To(Timers).Cancel(nameof(InitSave))
               from start in To(Timers).StartSingleTimer(nameof(InitSave), new InitSave(), TimeSpan.FromSeconds(1))
               select Unit.Instance);
        }

        private void SaveProject(SaveProject obj) 
            => Run(s => from state in s
                        where !state.IsSealed
                        select state with{ToSave = state.ToSave.Add((obj, Sender))});

        private static Either<Unit, Exception> TrySave(Maybe<ProjectFile> mayFile)
        {
            Maybe<Unit> MakeBackup()
            {
                Do(from file in mayFile
                   from exist in IO.File.Exists(May(file.Source))
                   where exist
                   from _ in IO.File.Copy(May(file.Source), May(file.Source + ".bal"), true)
                   select Unit.Instance);

                return Unit.MayInstance;
            }

            void StartSave()
                => Do(from file in mayFile
                      from _ in MakeBackup()
                      from io in IO.File.Open(MayNotEmpty(file.Source), FileMode.Create,
                                              stream => file.WriteData(stream.Select(s => new BinaryWriter(s))))
                      select Unit.Instance);
            
            return Try(StartSave);
        }

        private sealed record InitSave;
    }
}