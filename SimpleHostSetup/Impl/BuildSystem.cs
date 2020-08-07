using System;
using System.Threading.Tasks;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class BuildSystem
    {
        private readonly ILogger _logger = Log.ForContext<BuildSystem>();

        public async Task Run(BuildSystemConfiguration config)
        {
            _logger.Information("Begin Building Host Setup");
            try
            {
                _logger.Information("Select Apps");
                var finder = new ProjectFinder(config.SearchStart, config.SearchRootFile);

                var hostProject = finder.Search(config.HostProject);
                if(hostProject == null)
                    throw new InvalidOperationException("Host Project Not Found");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Build Setup");
            }
        }
    }
}