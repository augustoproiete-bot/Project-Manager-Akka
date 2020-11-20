using Functional.Maybe;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed record ForceSave(bool AndSeal, ProjectFile File)
    {
        public static Maybe<ForceSave> Force(Maybe<ProjectFile> file)
            => from f in file
               select new ForceSave(false, f);

        public static Maybe<ForceSave> Seal(Maybe<ProjectFile> file)
            => from f in file
               select new ForceSave(true, f);
    }
}