using System.Windows;
using JetBrains.Annotations;

namespace Tauron.Application.Wpf.UI
{
    [PublicAPI]
    public sealed class ViewModelBinding : ActorBinding
    {
        public ViewModelBinding(string name) 
            : base(name)
        {
            Converter = new ViewModelConverter();
        }
    }
}