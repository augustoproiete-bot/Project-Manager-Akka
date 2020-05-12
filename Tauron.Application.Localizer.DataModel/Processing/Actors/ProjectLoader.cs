using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ReceiveActor
    {
        public ProjectLoader()
        {
            Receive<InternalLoadProject>(LoadProjectFile);
        }

        private void LoadProjectFile(InternalLoadProject obj)
        {
            try
            {
                using var stream = File.OpenRead(obj.ProjectFile.Source);
                using var reader = new BinaryReader(stream);
                var projectFile = new ProjectFile(reader, obj.ProjectFile.Source, Sender);

                obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, projectFile, null, true));
            }
            catch (Exception e)
            {
                obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, new ProjectFile(ImmutableList<Project>.Empty, obj.ProjectFile.Source, Sender), e, false));
            }
            finally
            {
                Context.Stop(Self);
            }
        }
    }
}