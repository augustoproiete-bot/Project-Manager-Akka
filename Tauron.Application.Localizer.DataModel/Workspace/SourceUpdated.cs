namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class SourceUpdated
    {
        public string Source { get; }

        public SourceUpdated(string source)
        {
            Source = source;
        }
    }
}