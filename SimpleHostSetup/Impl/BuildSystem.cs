using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class BuildSystem
    {
        private readonly ILogger _logger = Log.ForContext<BuildSystem>();

        public async Task Run()
        {
            
            try
            {
                var finder = new ProjectFinder(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.CodeBase));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Build Setup");
            }
        }
    }
}