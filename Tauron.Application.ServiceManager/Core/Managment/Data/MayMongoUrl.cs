namespace Tauron.Application.ServiceManager.Core.Managment.Data
{
    public sealed class MayMongoUrl
    {
        public string Url { get; }

        public MayMongoUrl(string url) => Url = url;
    }
}