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

namespace MGIHelper.UI
{
    /// <summary>
    /// Interaktionslogik für AutoFanControl.xaml
    /// </summary>
    public partial class AutoFanControl : UserControl
    {
        public AutoFanControl()
        {
            InitializeComponent();
            Model = (AutoFanControlModel)DataContext;
        }

        private AutoFanControlModel Model;

        private async void Save_OnClick(object sender, RoutedEventArgs e)
            => await Model.Options.Save();

        private async void Load_OnClick(object sender, RoutedEventArgs e)
            => await Model.Options.Load();

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Model.Init();
        }
    }
}
