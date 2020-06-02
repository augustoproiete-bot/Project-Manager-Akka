namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class ImportProjectDialogResult
    {
        public ImportProjectDialogResult(string project)
        {
            Project = project;
        }

        public string Project { get; }
    }
}