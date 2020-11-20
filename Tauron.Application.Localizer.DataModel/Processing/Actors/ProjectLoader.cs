using System;
using System.IO;
using Akka.Actor;
using Functional.Maybe;
using Tauron.Akka;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ExposedReceiveActor
    {
        public ProjectLoader() 
            => Flow<InternalLoadProject>(b => b.Func(LoadProjectFile).ToRefFromMsg(p => p.Select(l => l.OriginalSender)));

        private Maybe<LoadedProjectFile> LoadProjectFile(Maybe<InternalLoadProject> obj)
        {


            //try
            //{
            //    using var stream = File.OpenRead(obj.ProjectFile.Source);
            //    using var reader = new BinaryReader(stream);
            //    var projectFile = ProjectFile.ReadFile(reader, obj.ProjectFile.Source, Sender);

            //    obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, projectFile, null, true));
            //}
            //catch (Exception e)
            //{
            //    obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, ProjectFile.FromSource(obj.ProjectFile.Source, Sender), e, false));
            //}
            //finally
            //{
            //    Context.Stop(Self);
            //}
        }
    }
}