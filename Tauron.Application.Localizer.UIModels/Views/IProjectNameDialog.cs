using System.Collections.Generic;
using Akka.Actor;
using MahApps.Metro.Controls.Dialogs;

namespace Tauron.Application.Localizer.UIModels.Views
{
    public interface IProjectNameDialog
    {
        BaseMetroDialog Dialog { get; }

        void Init(IEnumerable<string> projects, IActorRef resultResponder);
    }
}