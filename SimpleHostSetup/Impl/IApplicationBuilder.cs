using System.Threading.Tasks;

namespace SimpleHostSetup.Impl
{
    public interface IApplicationBuilder
    {
        Task<bool> BuildApplication(string project, string output);
    }
}