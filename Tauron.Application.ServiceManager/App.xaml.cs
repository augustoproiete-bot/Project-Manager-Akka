using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Syncfusion.SfSkinManager;
using Syncfusion.Windows.Shared;

namespace Tauron.Application.ServiceManager
{
    /// <summary>
    /// Interaction logic for Apps.xaml
    /// </summary>
    public partial class App
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            SfSkinManager.ApplyStylesOnApplication = true;
        }
    }
}
