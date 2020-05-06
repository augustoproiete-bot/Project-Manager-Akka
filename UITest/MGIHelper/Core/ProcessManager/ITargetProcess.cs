using System.Collections.Generic;
using System.Diagnostics;

namespace MGIHelper.Core.ProcessManager
{
    public interface ITargetProcess
    {
        IEnumerable<string> FileNames { get; }

        void Found(Process p);

        void Exit(Process p);
    }
}