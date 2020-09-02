using System;
using System.IO;
using System.Threading;
using System.Windows;
using Akka.Configuration;

namespace Tauron.Application.ServiceManager.PreAppStart
{
    public static class IpConfigurationChecker
    {
        public static bool? CheckConfiguration()
        {
            try
            {
                var baseConfig = ConfigurationFactory.ParseString(File.ReadAllText("seed.conf"));

                var value = baseConfig.GetString("akka.remote.dot-netty.tcp.hostname");
                if (!string.IsNullOrWhiteSpace(value))
                    return true;

                Thread uiThread = new Thread(() =>
                {

                    var app = new App();
                    app.Startup += (sender, args) => new IPAsker(s =>
                    {
                        value = s;
                        app.Shutdown();
                    }).Show();

                    app.Run();
                });

                uiThread.SetApartmentState(ApartmentState.STA);
                uiThread.Start();
                uiThread.Join();

                if (string.IsNullOrWhiteSpace(value))
                    return null;

                baseConfig = ConfigurationFactory.ParseString($"akka.remote.dot-netty.tcp.hostname = {value}").WithFallback(baseConfig);

                File.WriteAllText("seed.conf", baseConfig.ToString(true));
                return false;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                return null;
            }
        }
    }
}