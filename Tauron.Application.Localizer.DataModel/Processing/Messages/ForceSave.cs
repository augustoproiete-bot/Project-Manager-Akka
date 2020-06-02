namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class ForceSave
    {
        private ForceSave(bool andSeal, ProjectFile file)
        {
            AndSeal = andSeal;
            File = file;
        }

        public bool AndSeal { get; }

        public ProjectFile File { get; }

        public static ForceSave Force(ProjectFile file)
        {
            return new ForceSave(false, file);
        }

        public static ForceSave Seal(ProjectFile file)
        {
            return new ForceSave(true, file);
        }
    }
}