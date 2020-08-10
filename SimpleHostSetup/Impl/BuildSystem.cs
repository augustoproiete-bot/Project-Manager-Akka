using System;
using System.IO;
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
            string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "SimpleHostSetup");
            try
            {


                _logger.Information("Select Apps");
                var finder = new ProjectFinder(config.SearchStart, config.SearchRootFile);

                var hostProject = finder.Search(config.HostProject);
                if (hostProject == null)
                    throw new InvalidOperationException("Host Project Not Found");

                _logger.Information("Building Host Application");
                var builder = config.BuilderFactory();
                string hostOutput = Path.Combine(basePath, "Host");

                if(Directory.Exists(hostOutput))
                    Directory.Delete(hostOutput, true);
                Directory.CreateDirectory(hostOutput);

                var result = await builder.BuildApplication(hostProject, hostOutput);
                if (!result)
                {
                    _logger.Error("Host Project Build Failed");
                    return;
                }

                

            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Build Setup");
            }
            finally
            {
                Directory.Delete(basePath, true);
            }
        }
    }
}