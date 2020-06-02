namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class SaveProject : Operation
    {
        public SaveProject(string operationId, ProjectFile projectFile) : base(operationId)
        {
            ProjectFile = projectFile;
        }

        public ProjectFile ProjectFile { get; }
    }
}