using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class BuildSystem
    {
        private readonly ILogger _logger = Log.ForContext<BuildSystem>();
        private readonly AppPacker _appPacker = new AppPacker();

        public async Task Run(BuildSystemConfiguration config)
        {
            _logger.Information("Begin Building Host Setup");
            string setupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Tauron", "SimpleHostSetup");

            if(Directory.Exists(setupPath))
                Directory.Delete(setupPath, true);

            string basePath = Path.Combine(setupPath, "Build");
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

                Directory.CreateDirectory(hostOutput);

                var result = await builder.BuildApplication(hostProject, hostOutput);
                if (!result)
                {
                    _logger.Error("Host Project Build Failed");
                    return;
                }

                var hostConfig = new Configurator(hostOutput);
                string hostIp = config.Input.GetIp();
                string seedIp = config.Input.GetSeed();

                hostConfig.SetSeed(seedIp);
                hostConfig.SetIp(hostIp);
                hostConfig.SetAppName(config.Input.GetName("host"));

                await hostConfig.Save();

                await File.WriteAllTextAsync(Path.Combine(basePath, "StartHost.bat"), "cd %~dp0\\Host\n" +
                                                                                      "ServiceHost.exe\n" +
                                                                                      "pause");

                string appsOutput = Path.Combine(setupPath, "AppsBuild");

                foreach (var installApp in config.Input.GetAppsToInstall(config.AppMapping.Keys))
                {
                    var installAppData = config.AppMapping[installApp];
                    var appProject = finder.Search(installAppData.ProjectName);
                    if (appProject == null)
                        throw new InvalidOperationException($"App Project Not Found: {installApp} | {installAppData.ProjectName}");

                    _logger.Information("Building App {InstallApp}", installApp);
                    string appOutput = Path.Combine(appsOutput, installApp);

                    Directory.CreateDirectory(appOutput);

                    var appResult = await builder.BuildApplication(appProject, appOutput);
                    if (!appResult)
                    {
                        _logger.Error("{InstallApp} Project Build Failed", installApp);
                        return;
                    }

                    var appConfig = new Configurator(appOutput);
                    string appName = config.Input.GetName(installApp);

                    appConfig.SetSeed(seedIp);
                    appConfig.SetIp(hostIp);
                    appConfig.SetAppName(appName);

                    await appConfig.Save();

                    _appPacker.MakeZip(appOutput, Path.Combine(basePath, installApp + ".zip"), installApp);

                    //InstallSeed.bat
                    //cd %~dp0\TestHost
                    //ServiceHost.exe --Install Manual --ZipFile..\Seed.zip --AppName Master-Seed --AppType StartUp
                    //pause
                    //

                    await File.WriteAllTextAsync(Path.Combine(basePath, $"Install{installApp}.bat"), "cd %~dp0\\Host\n" +
                                                                                          $"ServiceHost.exe --Install Manual --ZipFile ..\\{installApp}.zip --AppName {appName} --AppType {installAppData.AppType}\n" +
                                                                                          //$"del ..\\{installApp}.zip" +
                                                                                          //$"del ..\\Install{installApp}.bat" +
                                                                                          "pause");
                }

                string targetPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string targetFile = Path.Combine(setupPath, Path.Combine(targetPath, "HostSetup.zip"));
                if(File.Exists(targetFile))
                    File.Delete(targetFile);

                _appPacker.MakeZip(basePath, targetFile, "Host");

                Process.Start("explorer.exe", targetPath);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error on Build Setup");

                Console.ReadKey();
            }
            finally
            {
                Directory.Delete(setupPath, true);
            }
        }
    }
}