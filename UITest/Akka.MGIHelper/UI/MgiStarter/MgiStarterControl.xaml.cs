using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.Wpf;

namespace Akka.MGIHelper.UI.MgiStarter
{
    /// <summary>
    /// Interaktionslogik für MgiStarterControl.xaml
    /// </summary>
    public partial class MgiStarterControl
    {
        public MgiStarterControl(IViewModel<MgiStarterControlModel> model)
            : base(model)
        {
            InitializeComponent();
        }
    }
}
