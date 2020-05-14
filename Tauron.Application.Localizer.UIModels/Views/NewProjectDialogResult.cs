namespace Tauron.Application.Localizer.UIModels.Views
{
    public sealed class NewProjectDialogResult
    {
        public string Name { get; }

        public NewProjectDialogResult(string name) 
            => Name = name;
    }
}