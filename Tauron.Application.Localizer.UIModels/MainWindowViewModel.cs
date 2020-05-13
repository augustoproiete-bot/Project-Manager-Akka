using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class MainWindowViewModel : UiActor
    {
        public static readonly string MainWindow = nameof(MainWindow);

        private readonly IOperationManager _operationManager;
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IDialogFactory _dialogFactory;

        private IEnumerable<RunningOperation> RunningOperations
        {
            set => Set(value);
        }

        private RenctFilesCollection RenctFiles
        {
            set => Set(value);
            get => Get<RenctFilesCollection>()!;
        }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager operationManager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            AppConfig config, IDialogFactory dialogFactory) 
            : base(lifetimeScope, dispatcher)
        {
            _operationManager = operationManager;
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            _dialogFactory = dialogFactory;
            RunningOperations = operationManager.RunningOperations;

            RenctFiles = new RenctFilesCollection(config);

            RegisterCommand("ClearOp", operationManager.Clear, operationManager.ShouldClear);
            RegisterCommand("ClearAllOp", operationManager.CompledClear, operationManager.ShouldCompledClear);
            RegisterCommand("OpenFile", OpenFile, () => _loadingOperation == null);
            RegisterCommand("OpenRenct", o =>
                                         {
                                             if(o is string file)
                                                 OpentFileSource(file);
                                         }, _ => _loadingOperation == null);
            RegisterCommand("NewFile", NewFile, () => _loadingOperation == null);
            RegisterCommand("SaveAs", SaveAsProject, () => _last != null);

            Receive<LoadedProjectFile>(ProjectLoaded);
            ReceiveAsync<SourceSelected>(async s =>
                                    {
                                        if(s.Mode == OpenFileMode.OpenExistingFile)
                                            OpentFileSource(s.Source);
                                        else
                                            await NewFileSource(s.Source);
                                    });
        }

        private void SaveAsProject()
        {
            var targetFile = _dialogFactory.ShowSaveFileDialog(null, true, true, true, "transp", true,
                _localizer.OpenFileDialogViewDialogFilter, true, true, _localizer.MainWindowMainMenuFileSaveAs, Directory.GetCurrentDirectory(), out var result);

            if(CheckSourceOk(targetFile)) return;

            UpdateSource(targetFile!);
        }

        private void NewFile()
        {
            var self = Self;
            Dispatcher.Invoke(async () =>
                              {
                                  var dialog = LifetimeScope.Resolve<IOpenFileView>(TypedParameter.From(new Action<string?>(s => self.Tell(new SourceSelected(s, OpenFileMode.OpenNewFile)))),
                                      TypedParameter.From(OpenFileMode.OpenNewFile)).Dialog;
                                  await _dialogCoordinator.ShowMetroDialogAsync(MainWindow, dialog);
                              });
        }

        private OperationController? _loadingOperation;
        private ProjectFile? _last;

        private void OpenFile()
        {
            var self = Self;
            Dispatcher.Invoke(async () =>
                              {
                                  var dialog = LifetimeScope.Resolve<IOpenFileView>(TypedParameter.From(new Action<string?>(s => self.Tell(new SourceSelected(s, OpenFileMode.OpenExistingFile)))),
                                      TypedParameter.From(OpenFileMode.OpenExistingFile)).Dialog;
                                  await _dialogCoordinator.ShowMetroDialogAsync(MainWindow, dialog);
                              });
        }

        private bool CheckSourceOk(string? source)
        {
            if (!string.IsNullOrWhiteSpace(source)) return false;
            Dispatcher.Invoke(async () => await _dialogCoordinator.ShowMessageAsync(MainWindow, _localizer.CommonError, _localizer.MainWindowModelLoadProjectSourceEmpty));
            return true;

        }

        private async Task NewFileSource(string? source)
        {
            if(CheckSourceOk(source)) return;

            if (File.Exists(source))
            {
                var result = await Dispatcher.Invoke(async () => await _dialogCoordinator.ShowMessageAsync(MainWindow, _localizer.CommonError, "", 
                                                              MessageDialogStyle.AffirmativeAndNegative));

                if(result == MessageDialogResult.Negative) return;
            }

            Self.Tell(new LoadedProjectFile(string.Empty, ProjectFile.NewProjectFile(Context, source!, "Project_Operator"), null, true));
        }

        private void OpentFileSource(string? source)
        {
            if(CheckSourceOk(source)) return;

            _loadingOperation = _operationManager.StartOperation(string.Format(_localizer.MainWindowModelLoadProjectOperation, Path.GetFileName(source) ?? source));
            ProjectFile.BeginLoad(Context, _loadingOperation.Id, source!, "Project_Operator");
        }

        private void ProjectLoaded(LoadedProjectFile obj)
        {
            if (_loadingOperation != null)
            {
                if(obj.Ok)
                    _loadingOperation.Compled();
                else
                {
                    _loadingOperation.Failed(obj.ErrorReason?.Message ?? _localizer.CommonError);
                    return;
                }
            }

            if(obj.Ok)
                RenctFiles.AddNewFile(obj.ProjectFile.Source);

            _last = obj.ProjectFile;

            //TODO Delegate To MainView
        }

        private void UpdateSource(string source)
        {

        }

        private sealed class RenctFilesCollection : UIObservableCollection<string>
        {
            private readonly AppConfig _config;

            public RenctFilesCollection(AppConfig config)
                : base(config.RenctFiles) 
                => _config = config;

            public void AddNewFile(string file)
            {
                if(Count > 10)
                    RemoveAt(Count - 1);
                
                Add(file);
                _config.RenctFiles = ImmutableList<string>.Empty.AddRange(this);
            }
        }
    }
}