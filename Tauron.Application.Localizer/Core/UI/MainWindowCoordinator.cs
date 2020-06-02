using System;
using Tauron.Application.Localizer.UIModels.Services;

namespace Tauron.Application.Localizer.Core.UI
{
    public sealed class MainWindowCoordinator : IMainWindowCoordinator
    {
        private bool _isBusy;
        private string _titlePostfix = string.Empty;

        public event Action? TitleChanged;
        public event Action? IsBusyChanged;

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

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                IsBusyChanged?.Invoke();
            }
        }
    }
}