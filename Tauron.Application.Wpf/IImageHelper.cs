using System;
using System.Windows.Media;

namespace Tauron.Application.Wpf
{
    public interface IImageHelper
    {
        ImageSource? Convert(Uri target, string assembly);

        ImageSource? Convert(string uri, string assembly);
    }
}