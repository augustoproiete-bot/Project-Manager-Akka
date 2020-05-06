using System;
using System.IO;
using System.Xml.Linq;

namespace MGIHelper.Core.Configuration
{
    public class ProcessConfig
    {
        public string Kernel { get; }

        public string Client { get; }

        private ProcessConfig(string path)
        {
            try
            {

                var ele = XElement.Parse(File.ReadAllText(path));
                Kernel = ele.Element("Kernel")?.Value.Trim() ?? string.Empty;
                Client = ele.Element("Client")?.Value.Trim() ?? string.Empty;
            }
            catch (Exception e)
            {
                Kernel = string.Empty;
                Client = string.Empty;
            }
        }

        public static  ProcessConfig Read() => new ProcessConfig(".\\ProcessConfig.xml");
    }
}