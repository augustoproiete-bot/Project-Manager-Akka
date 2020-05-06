using System;
using System.Windows;

namespace MGIHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var context = ((MainWindowViewModel) DataContext);
            await context.Init();

            Dispatcher?.Invoke(() =>
            {
                Top = context.WindowOptions.PositionY;
                Left = context.WindowOptions.PositionX;
            });
        }

        private void MainWindow_OnLocationChanged(object? sender, EventArgs e)
        {
            var context = ((MainWindowViewModel) DataContext);

            context.WindowOptions.PositionX = Left;
            context.WindowOptions.PositionY = Top;
        }
    }
}
