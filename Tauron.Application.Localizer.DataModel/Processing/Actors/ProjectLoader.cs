using System;
using System.IO;
using Functional.Either;
using Functional.Maybe;
using Tauron.Akka;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ExposedReceiveActor
    {
        public ProjectLoader() 
            => Flow<InternalLoadProject>(b => b.Func(LoadProjectFile).ToRefFromMsg(p => p.Select(l => l.OriginalSender)));

        private Maybe<LoadedProjectFile> LoadProjectFile(Maybe<InternalLoadProject> obj)
        {
            Maybe<ProjectFile> LoadProject(string source)
            {
                return IO.File.Open(May(source), FileMode.Open,
                                    s => ProjectFile.ReadFile(
                                                              s.Select(ss => new BinaryReader(ss)),
                                                              May(source), 
                                                              May(Sender)));
            }
            
            return from load in obj
                   let id = load.ProjectFile.OperationId
                   let source = load.ProjectFile.Source
                   select Finally(() =>
                                      Match(
                                            Try(() =>
                                                    from proj in LoadProject(source) 
                                                    select new LoadedProjectFile(id, proj, Maybe<Exception>.Nothing, true)),
                                            e => new LoadedProjectFile(id, ProjectFile.FromSource(source, Sender), May(e), false)),
                                  () => Context.Stop(Self));
        }
    }
}