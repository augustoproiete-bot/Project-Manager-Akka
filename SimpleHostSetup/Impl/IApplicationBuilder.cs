using System.IO;
using System.Threading.Tasks;

namespace SimpleHostSetup.Impl
{
    public interface IApplicationBuilder
    {
        Task<bool> BuildApplication(FileInfo project, string output);
    }
}