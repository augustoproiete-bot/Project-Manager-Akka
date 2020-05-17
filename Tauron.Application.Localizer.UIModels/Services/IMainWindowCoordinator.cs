using System;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IMainWindowCoordinator
    {
        public event Action TitleChanged;

        string TitlePostfix { set; get; }

        bool Saved { get; set; }
    }
}