using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Tauron.Application.Localizer.UIModels;
using Tauron.Application.Wpf;
using UserControl = System.Windows.Controls.UserControl;

namespace Tauron.Application.Localizer.Views
{
    /// <summary>
    /// Interaktionslogik für CenterView.xaml
    /// </summary>
    public partial class CenterView
    {
        public CenterView(IViewModel<CenterViewModel> model)
            : base(model)
        {
            InitializeComponent();
            Background = Brushes.Transparent;
        }
    }
}
