using System;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IMainWindowCoordinator
    {
        string TitlePostfix { set; get; }

        bool Saved { get; set; }

        bool IsBusy { get; set; }
        public event Action TitleChanged;

        public event Action IsBusyChanged;
    }
}