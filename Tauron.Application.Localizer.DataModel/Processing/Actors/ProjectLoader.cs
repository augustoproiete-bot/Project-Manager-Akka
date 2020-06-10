using System;
using System.IO;
using Akka.Actor;
using Tauron.Akka;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectLoader : ExposedReceiveActor
    {
        public ProjectLoader() 
            => this.Flow<InternalLoadProject>().To.Action(LoadProjectFile).AndReceive();

        private void LoadProjectFile(InternalLoadProject obj)
        {
            try
            {
                using var stream = File.OpenRead(obj.ProjectFile.Source);
                using var reader = new BinaryReader(stream);
                var projectFile = ProjectFile.ReadFile(reader, obj.ProjectFile.Source, Sender);

                obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, projectFile, null, true));
            }
            catch (Exception e)
            {
                obj.OriginalSender.Tell(new LoadedProjectFile(obj.ProjectFile.OperationId, ProjectFile.FromSource(obj.ProjectFile.Source, Sender), e, false));
            }
            finally
            {
                Context.Stop(Self);
            }
        }
    }
}