namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class BuildRequest
    {
        public string OperationId { get; }

        public ProjectFile ProjectFile { get; }

        public BuildRequest(string operationId, ProjectFile projectFile)
        {
            OperationId = operationId;
            ProjectFile = projectFile;
        }
    }
}