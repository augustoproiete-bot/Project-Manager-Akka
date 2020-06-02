using System;
using System.IO;
using System.Windows.Input;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class RenctFile
    {
        public RenctFile(string file, Action<string> loadFileAction)
        {
            File = file;
            Name = Path.GetFileName(file);
            Runner = new SimpleCommand(() => loadFileAction(file));
        }

        public string File { get; }
        public string Name { get; }

        public ICommand Runner { get; }
    }
}