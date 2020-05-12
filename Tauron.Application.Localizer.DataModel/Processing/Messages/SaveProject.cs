using JetBrains.Annotations;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class SaveProject : Operation
    {
        public ProjectFile ProjectFile { get; }

        public SaveProject(string operationId, ProjectFile projectFile) : base(operationId) => ProjectFile = projectFile;
    }
}