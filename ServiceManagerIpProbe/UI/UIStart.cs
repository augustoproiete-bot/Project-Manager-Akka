using System;
using System.Windows.Forms;

namespace ServiceManagerIpProbe.UI
{
    public static class UIStart
    {
        public static void Run(Action<Action<string>> runInstall)
        {
            Application.EnableVisualStyles();
            Application.Run(new Info(runInstall));
        }
    }
}