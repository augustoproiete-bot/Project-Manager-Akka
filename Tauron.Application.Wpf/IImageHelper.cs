using System;
using System.Windows.Media;
using Functional.Maybe;

namespace Tauron.Application.Wpf
{
    public interface IImageHelper
    {
        Maybe<ImageSource> Convert(Uri target, string assembly);

        Maybe<ImageSource> Convert(string uri, string assembly);
    }
}