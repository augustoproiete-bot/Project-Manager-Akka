namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record SaveProject(ProjectFile ProjectFile, string OperationId) : Operation(OperationId);
}