namespace ServiceManager.ProjectRepository.Core
{
    public interface IReporterProvider
    {
        void SendMessage(string msg);
    }
}