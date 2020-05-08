namespace Tauron.Host
{
    public interface IHostingEnvironment
    {
        string EnvironmentName
        {
            get;
            set;
        }

        string ApplicationName
        {
            get;
            set;
        }

        string ContentRootPath
        {
            get;
            set;
        }
    }
}