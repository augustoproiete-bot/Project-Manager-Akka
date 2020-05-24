using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels.Views
{
    [PublicAPI]
    public static class BaseDialogExtension
    {
        public static Func<TData> ShowDialog<TDialog, TData>(this UiActor actor, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TData>
            => ShowDialog<TDialog, TData, TData>(actor, Array.Empty<TData>(), parameters);

        public static Func<TData> ShowDialog<TDialog, TData, TViewData>(this UiActor actor, IEnumerable<TViewData> initalData, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TViewData>
        {
            return () =>
                   {
                       Task<TData>? task = null;
                       BaseMetroDialog? metroDialog = null;

                       actor.Dispatcher.Invoke(async () =>
                                               {
                                                   var dialog = actor.LifetimeScope.Resolve<TDialog>(parameters);
                                                   task = dialog.Init(initalData);
                                                   metroDialog = dialog.Dialog;

                                                   await DialogCoordinator.Instance.ShowMetroDialogAsync("MainWindow", metroDialog);
                                               });

                       var result = task!.Result;

                       actor.Dispatcher.Invoke(async () => await DialogCoordinator.Instance.HideMetroDialogAsync("MainWindow", metroDialog!));

                       return result;
                   };
        }
    }
}