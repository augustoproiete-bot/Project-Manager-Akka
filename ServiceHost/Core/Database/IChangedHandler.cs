namespace ServiceHost.Core.Database
{
    public interface IChangedHandler
    {
        void Changed(ChangeType type, string key, string oldContent, string content);
    }
}