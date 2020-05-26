namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class ImportProjectDialogResult
    {
        public string Project { get; }

        public ImportProjectDialogResult(string project) 
            => Project = project;
    }
}