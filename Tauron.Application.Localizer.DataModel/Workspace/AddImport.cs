namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class AddImport
    {
        public AddImport(string projectName, string import)
        {
            ProjectName = projectName;
            Import = import;
        }

        public string ProjectName { get; }

        public string Import { get; }
    }
}