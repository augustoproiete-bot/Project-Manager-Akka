using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels.Views
{
    [PublicAPI]
    public static class BaseDialogExtension
    {
        private static IDialogCoordinator? _dialogCoordinator;

        public static Func<TData> ShowDialog<TDialog, TData>(this UiActor actor, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TData>
        {
            return ShowDialog<TDialog, TData, TData>(actor, Array.Empty<TData>, parameters);
        }

        public static Func<TData> ShowDialog<TDialog, TData, TViewData>(this UiActor actor, Func<IEnumerable<TViewData>> initalData, params Parameter[] parameters)
            where TDialog : IBaseDialog<TData, TViewData>
        {
            _dialogCoordinator ??= actor.LifetimeScope.Resolve<IDialogCoordinator>();

            return () =>
            {
                Task<TData>? task = null;

                actor.Dispatcher.Invoke(() =>
                {
                    var dialog = actor.LifetimeScope.Resolve<TDialog>(parameters);
                    task = dialog.Init(initalData());

                    _dialogCoordinator.ShowDialog(dialog);
                });

                var result = task!.Result;

                actor.Dispatcher.Invoke(() => _dialogCoordinator.HideDialog());

                return result;
            };
        }
    }
}