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

        public async Task Run(BuildSystemConfiguration config)
        {
            
            try
            {
                var finder = new ProjectFinder(config.SearchStart, config.SearchRootFile);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Build Setup");
            }
        }
    }
}