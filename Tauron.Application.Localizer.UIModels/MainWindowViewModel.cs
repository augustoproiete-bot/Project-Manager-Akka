using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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
        private readonly IMainWindowCoordinator _mainWindowCoordinator;

        private IEnumerable<RunningOperation> RunningOperations
        {
            set => Set(value);
        }

        private RenctFilesCollection RenctFiles
        {
            set => Set(value);
            get => Get<RenctFilesCollection>()!;
        }

        private IViewModel<CenterViewModel> CenterView
        {
            get => Get<IViewModel<CenterViewModel>>()!;
            set => Set(value);
        }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager operationManager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            AppConfig config, IDialogFactory dialogFactory, IViewModel<CenterViewModel> model, IMainWindowCoordinator mainWindowCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            model.Init(Context, "CenterView");
            CenterView = model;

            _operationManager = operationManager;
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            _dialogFactory = dialogFactory;
            _mainWindowCoordinator = mainWindowCoordinator;
            RunningOperations = operationManager.RunningOperations;

            var self = Self;
            RenctFiles = new RenctFilesCollection(config, s => self.Tell(new InternlRenctFile(s)));

            RegisterCommand("ClearOp", operationManager.Clear, operationManager.ShouldClear);
            RegisterCommand("ClearAllOp", operationManager.CompledClear, operationManager.ShouldCompledClear);
            RegisterCommand("OpenFile", OpenFile, () => _loadingOperation == null);
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

            Receive<InternlRenctFile>(o => OpentFileSource(o.File));
        }

        private void SaveAsProject()
        {
            var targetFile = _dialogFactory.ShowSaveFileDialog(null, true, true, true, "transp", true,
                _localizer.OpenFileDialogViewDialogFilter, true, true, _localizer.MainWindowMainMenuFileSaveAs, Directory.GetCurrentDirectory(), out var result);

            if(result != true && CheckSourceOk(targetFile)) return;

            UpdateSource(targetFile!);
        }

        private void NewFile()
        {
            var self = Self;
            Dispatcher.Invoke(async () =>
                              {
                                  var dialog = LifetimeScope.Resolve<IOpenFileDialog>(TypedParameter.From(new Action<string?>(s => self.Tell(new SourceSelected(s, OpenFileMode.OpenNewFile)))),
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
                                  var dialog = LifetimeScope.Resolve<IOpenFileDialog>(TypedParameter.From(new Action<string?>(s => self.Tell(new SourceSelected(s, OpenFileMode.OpenExistingFile)))),
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
            source ??= string.Empty;

            if (File.Exists(source))
            {
                var result = await Dispatcher.Invoke(async () => await _dialogCoordinator.ShowMessageAsync(MainWindow, _localizer.CommonError, "", 
                                                              MessageDialogStyle.AffirmativeAndNegative));

                if(result == MessageDialogResult.Negative) return;
            }

            _mainWindowCoordinator.IsBusy = true;
            Self.Tell(new LoadedProjectFile(string.Empty, ProjectFile.NewProjectFile(Context, source, "Project_Operator"), null, true));
        }

        private void OpentFileSource(string? source)
        {
            if(CheckSourceOk(source)) return;

            _mainWindowCoordinator.IsBusy = true;
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
                    _mainWindowCoordinator.IsBusy = false;
                    _loadingOperation.Failed(obj.ErrorReason?.Message ?? _localizer.CommonError);
                    return;
                }
            }

            if (obj.Ok) RenctFiles.AddNewFile(obj.ProjectFile.Source);

            _last = obj.ProjectFile;

            CenterView.Actor.Tell(new SupplyNewProjectFile(_last));
        }

        private void UpdateSource(string source) 
            => CenterView.Actor.Tell(new UpdateSource(source));

        private sealed class RenctFilesCollection : UIObservableCollection<RenctFile>
        {
            private readonly AppConfig _config;
            private readonly Action<string> _loader;

            public RenctFilesCollection(AppConfig config, Action<string> loader)
                : base(config.RenctFiles.Select(s => new RenctFile(s.Trim(), loader)))
            {
                _config = config;
                _loader = loader;
            }

            public void AddNewFile(string file)
            {
                file = file.Trim();

                if(this.Any(rf => rf.File == file)) return;

                if(Count > 10)
                    RemoveAt(Count - 1);
                
                Add(new RenctFile(file, _loader));
                _config.RenctFiles = ImmutableList<string>.Empty.AddRange(this.Select(rf => rf.File));
            }
        }

        private sealed class InternlRenctFile
        {
            public string File { get; }

            public InternlRenctFile(string file) => File = file;
        }
    }
}