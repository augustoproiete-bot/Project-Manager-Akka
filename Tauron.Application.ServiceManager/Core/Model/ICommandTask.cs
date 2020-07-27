using System;
using System.Threading.Tasks;

namespace Tauron.Application.ServiceManager.Core.Model
{
    public interface ICommandTask
    {
        Task<bool> Run();

        void ReportError(Exception? e);

        void Finish();
    }
}