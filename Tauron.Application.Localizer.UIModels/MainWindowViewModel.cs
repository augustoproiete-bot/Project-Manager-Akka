using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class MainWindowViewModel : UiActor
    {
        public static readonly string MainWindow = nameof(MainWindow);

        private readonly IOperationManager _operationManager;
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;

        private IEnumerable<RunningOperation> RunningOperations
        {
            set => Set(value);
        }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager operationManager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            _operationManager = operationManager;
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            RunningOperations = operationManager.RunningOperations;

            RegisterCommand("ClearOp", operationManager.Clear, operationManager.ShouldClear);
            RegisterCommand("ClearAllOp", operationManager.CompledClear, operationManager.ShouldCompledClear);
            RegisterCommand("OpenFile", OpenFile, () => _loadingOperation == null);

            Receive<LoadedProjectFile>(ProjectLoaded);
            Receive<SourceSelected>(s => OpentFileSource(s.Source));
        }

        private OperationController? _loadingOperation;

        private void OpenFile()
        {
            var self = Self;
            Dispatcher.Invoke(async () =>
                              {
                                  var dialog = LifetimeScope.Resolve<IOpenFileView>(TypedParameter.From(new Action<string?>(s => self.Tell(new SourceSelected(s))))).Dialog;
                                  await _dialogCoordinator.ShowMetroDialogAsync(MainWindow, dialog);
                              });
        }

        private void OpentFileSource(string source)
        {
            _loadingOperation = _operationManager.StartOperation(string.Format(_localizer.MainWindowModelLoadProjectOperation, Path.GetFileName(source) ?? source));
        }

        private void ProjectLoaded(LoadedProjectFile obj)
        {
            
        }
    }
}