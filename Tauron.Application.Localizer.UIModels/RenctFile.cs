using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class RenctFile
    {
        public string File { get; }
        public string Name { get; }

        public ICommand Runner { get; }

        public RenctFile(string file, Action<string> loadFileAction)
        {
            File = file;
            Name = Path.GetFileName(file);
            Runner = new ActionCommand(() => loadFileAction(file));
        }
    }
}