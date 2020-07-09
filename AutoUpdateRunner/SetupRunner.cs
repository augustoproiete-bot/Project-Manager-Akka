using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Serilog;

namespace AutoUpdateRunner
{
    public sealed class SetupRunner
    {
        private readonly ILogger _logger = Log.ForContext<SetupRunner>();

        private readonly SetupInfo _info;

        public SetupRunner(SetupInfo info) => _info = info;

        public async Task Run()
        {
            var backup = Path.GetFullPath("Backup");
            var failed = false;

            try
            {

            }
            catch (Exception e)
            {
                _logger.Error(e, "Error Running Copy");
                failed = true;
            }
            finally
            {
                if(failed)
                    Revert(_info.Target, backup);
                StartHost();
            }
        }

        private void Revert(string target, string backup)
        {

        }

        private void StartHost()
        {
            try
            {
                Process.Start(_info.StartFile);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}