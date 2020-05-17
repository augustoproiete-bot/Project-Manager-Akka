using System;
using Tauron.Application.Localizer.UIModels.Services;

namespace Tauron.Application.Localizer.Core.UI
{
    public sealed class MainWindowCoordinator : IMainWindowCoordinator
    {
        private string _titlePostfix = string.Empty;

        public event Action? TitleChanged;

        public string TitlePostfix
        {
            get => _titlePostfix;
            set
            {
                _titlePostfix = value;
                TitleChanged?.Invoke();
            }
        }

        public bool Saved { get; set; } = true;
    }
}