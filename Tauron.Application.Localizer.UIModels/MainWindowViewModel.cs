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
        private UIProperty<IEnumerable<RunningOperation>> RunningOperations { get; }

        private UIProperty<RenctFilesCollection> RenctFiles { get; }

        private UIModel<CenterViewModel> CenterView { get; }

        public MainWindowViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager operationManager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            AppConfig config, IDialogFactory dialogFactory, IViewModel<CenterViewModel> model, IMainWindowCoordinator mainWindowCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            var self = Self;
            CenterView = this.RegisterViewModel(nameof(CenterView), model);
            
            #region Operation Manager

            RunningOperations = RegisterProperty<IEnumerable<RunningOperation>>(nameof(RunningOperations)).WithDefaultValue(operationManager.RunningOperations);
            RenctFiles = RegisterProperty<RenctFilesCollection>(nameof(RenctFiles)).WithDefaultValue(new RenctFilesCollection(config, s => self.Tell(new InternlRenctFile(s))));
            NewCommad.WithExecute(operationManager.Clear, operationManager.ShouldClear).ThenRegister("ClearOp");
            NewCommad.WithExecute(operationManager.CompledClear, operationManager.ShouldCompledClear).ThenRegister("ClearAllOp");

            #endregion

            #region Save As

            UpdateSource? SaveAsProject()
            {
                var targetFile = dialogFactory.ShowSaveFileDialog(null, true, true, true, "transp", true,
                    localizer.OpenFileDialogViewDialogFilter, true, true, localizer.MainWindowMainMenuFileSaveAs, Directory.GetCurrentDirectory(), out var result);

                if (result != true && CheckSourceOk(targetFile)) return null;

                return new UpdateSource(targetFile!);
            }

            bool CheckSourceOk(string? source)
            {
                if (!string.IsNullOrWhiteSpace(source)) return false;
                UICall(() => dialogCoordinator.ShowMessage(localizer.CommonError, localizer.MainWindowModelLoadProjectSourceEmpty));
                return true;

            }

            NewCommad.WithCanExecute(() => _last != null)
               .ToFlow(SaveAsProject).Send.ToModel(CenterView);

            #endregion

            #region Open File

            Receive<InternlRenctFile>(o => OpentFileSource(o.File));

            async Task<LoadedProjectFile?> SourceSelectedFunc(SourceSelected s)
            {
                if (s.Mode != OpenFileMode.OpenExistingFile) return await NewFileSource(s.Source);
                OpentFileSource(s.Source);
                return null;
            }

            void OpentFileSource(string? source)
            {
                if (CheckSourceOk(source)) return;

                mainWindowCoordinator.IsBusy = true;
                _loadingOperation = operationManager.StartOperation(string.Format(localizer.MainWindowModelLoadProjectOperation, Path.GetFileName(source) ?? source));
                ProjectFile.BeginLoad(Context, _loadingOperation.Id, source!, "Project_Operator");
            }

            SupplyNewProjectFile? ProjectLoaded(LoadedProjectFile obj)
            {
                if (_loadingOperation != null)
                {
                    if (obj.Ok)
                        _loadingOperation.Compled();
                    else
                    {
                        mainWindowCoordinator.IsBusy = false;
                        _loadingOperation.Failed(obj.ErrorReason?.Message ?? localizer.CommonError);
                        return null;
                    }
                }

                if (obj.Ok) RenctFiles.Value.AddNewFile(obj.ProjectFile.Source);

                _last = obj.ProjectFile;

                return new SupplyNewProjectFile(_last);
            }

            NewCommad.WithCanExecute(() => _loadingOperation == null)
               .ToFlow(SourceSelected.From(this.ShowDialog<IOpenFileDialog, string?>(TypedParameter.From(OpenFileMode.OpenExistingFile)), OpenFileMode.OpenExistingFile))
               .To.Func(SourceSelectedFunc).ToSelf()
               .Then.Func(ProjectLoaded!).ToModel(CenterView)
               .Then.Return().ThenRegister("OpenFile");

            #endregion

            #region New File
            
            async Task<LoadedProjectFile?> NewFileSource(string? source)
            {
                source ??= string.Empty;

                if (File.Exists(source))
                {
                    //TODO NewFile Filog Message
                    var result = await UICall(async () => await dialogCoordinator.ShowMessage(localizer.CommonError, "", null));

                    if (result != true) return null;
                }

                mainWindowCoordinator.IsBusy = true;
                return new LoadedProjectFile(string.Empty, ProjectFile.NewProjectFile(Context, source, "Project_Operator"), null, true);
            }

            NewCommad.WithCanExecute(() => _loadingOperation == null)
               //.ToFlow(SourceSelected.From(() => "", OpenFileMode.OpenNewFile)).Send.ToSelf()
               .ToFlow(SourceSelected.From(this.ShowDialog<IOpenFileDialog, string?>(TypedParameter.From(OpenFileMode.OpenNewFile)), OpenFileMode.OpenNewFile)).Send.ToSelf()
               .Return().ThenRegister("NewFile");

            #endregion
        }

        private OperationController? _loadingOperation;
        private ProjectFile? _last;


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

                if(string.IsNullOrWhiteSpace(file) || !File.Exists(file)) return;

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