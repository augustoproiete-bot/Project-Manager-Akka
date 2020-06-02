namespace Tauron.Application.Localizer.DataModel.Workspace
{
    public sealed class SourceUpdated
    {
        public SourceUpdated(string source)
        {
            Source = source;
        }

        public string Source { get; }
    }
}