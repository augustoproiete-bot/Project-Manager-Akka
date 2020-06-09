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
using Tauron.Akka;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für BuildView.xaml
    /// </summary>
    public partial class BuildView
    {
        public BuildView(IViewModel<BuildViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}
