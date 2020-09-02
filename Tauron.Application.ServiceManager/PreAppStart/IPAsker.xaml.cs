using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Tauron.Application.ServiceManager.PreAppStart
{
    /// <summary>
    /// Interaktionslogik für IPAsker.xaml
    /// </summary>
    public partial class IPAsker : Window
    {
        private readonly Action<string?> _ipResult;
        private bool _called;

        public IPAsker(Action<string?> ipResult)
        {
            _ipResult = s =>
            {
                _called = true;
                ipResult(s);
            };
            InitializeComponent();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            _ipResult(null);
            Close();
        }

        private void StartClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var adresss = IPAddress.Parse(IpBox.Text);

                if (adresss.AddressFamily == AddressFamily.InterNetwork)
                    _ipResult(IpBox.Text);
                else
                    IpBox.Text = "Only IPv4";
            }
            catch (Exception exception)
            {
                IpBox.Text = $"Invalid: {exception.Message}";
            }
        }

        private void IPAsker_OnClosed(object? sender, EventArgs e)
        {
            if(_called) return;
            _ipResult(null);
        }

        private void TryFindClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                string localIp = "Unkowen";
                try
                {
                    using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                    socket.Connect("8.8.8.8", 65530);
                    var endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIp = endPoint == null ? "Not Found" : endPoint.Address.ToString();
                }
                catch (Exception exception)
                {
                    localIp = $"Error: {exception.Message}";
                }

                Dispatcher.Invoke(() =>
                {
                    IpBox.Text = localIp;
                });
            });
        }
    }
}
