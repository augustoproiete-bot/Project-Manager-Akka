using Akka.Actor;
using Tauron.Application.Localizer.DataModel;

namespace Tauron.Application.Localizer.UIModels.Services.Data
{
    public sealed class ProjectFileWorkspace
    {
        

        public ProjectFile ProjectFile { get; }

        public ProjectFileWorkspace(ProjectFile projectFile) 
            => ProjectFile = projectFile;

        public void ChangeSource(string newSource)
        {

        }

        public void RevertSource()
        {

        }
    }
}