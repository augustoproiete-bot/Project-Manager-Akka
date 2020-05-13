using System;
using System.IO;
using Akka.Actor;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class ProjectSaver : ReceiveActor
    {
        public ProjectSaver()
        {
            Receive<SaveProject>(SaveProject);
        }

        private void SaveProject(SaveProject obj)
        {
            try
            {
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
    }
}