using System.IO;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class CenterViewModel : UiActor
    {
        private readonly IOperationManager _manager;
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IMainWindowCoordinator _mainWindow;
        private readonly ProjectFileWorkspace _workspace;

        private UICollectionProperty<ProjectViewContainer> Views { get; }

        private UIProperty<int?> CurrentProject { get; set; }

        public CenterViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager manager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
                            IMainWindowCoordinator mainWindow, ProjectFileWorkspace workspace) 
            : base(lifetimeScope, dispatcher)
        {
            Views = this.RegisterUiCollection<ProjectViewContainer>(nameof(Views)).Async();
            CurrentProject = RegisterProperty<int?>(nameof(CurrentProject));

            _manager = manager;
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            _mainWindow = mainWindow;
            _workspace = workspace;

            NewCommad.WithExecute(TryRemoveProject, () => !_workspace.ProjectFile.IsEmpty && CurrentProject != null).ThenRegister("RemoveProject");

            this.RespondOnEventSource(_workspace.Source.SaveRequest, SaveRequested);
            Receive<SavedProject>(ProjectSaved);

            this.RespondOnEventSource(_workspace.Projects.RemovedProject, p => RemoveProject(p.Project));
            this.RespondOnEventSource(_workspace.Source.SourceUpdate, updated => _mainWindow.TitlePostfix = Path.GetFileName(updated.Source));

            Receive<UpdateSource>(UpdateSourceHandler);
            Receive<NewProjectDialogResult>(NewProjectDialogResult);

            #region Project Reset

            void ProjectRest(ProjectRest obj)
            {
                _mainWindow.Saved = File.Exists(obj.ProjectFile.Source);

                foreach (var view in Views)
                    Context.Stop(view.Model.Actor);
                Views.Clear();

                string titleName = obj.ProjectFile.Source;
                if (string.IsNullOrWhiteSpace(titleName))
                    titleName = _localizer.CommonUnkowen;
                else
                {
                    titleName = Path.GetFileNameWithoutExtension(obj.ProjectFile.Source);
                    if (string.IsNullOrWhiteSpace(titleName))
                        titleName = _localizer.CommonUnkowen;
                }

                _mainWindow.TitlePostfix = titleName;

                foreach (var project in obj.ProjectFile.Projects)
                    AddProject(project);

                CommandChanged();
                _mainWindow.IsBusy = false;
            }

            this.Flow<SupplyNewProjectFile>().To.Mutate(_workspace.Source).For(sm => sm.ProjectReset, sm => np => sm.Reset(np.File)).ToSelf()
               .Then.Action(ProjectRest);

            #endregion

            #region Addproject

            void AddProject(Project project)
            {
                string name = GetActorName(project.ProjectName);
                if (!ActorPath.IsValidPathElement(name))
                {
                    UICall(async c => await _dialogCoordinator.ShowMessageAsync("MainWindow", _localizer.CommonError, _localizer.CenterViewNewProjectInvalidNameMessage));
                    return;
                }

                var view = LifetimeScope.Resolve<IViewModel<ProjectViewModel>>();
                view.Init(Context, name);
                view.Actor.Tell(new InitProjectViewModel(project), Self);

                Views.Add(new ProjectViewContainer(view, project));

                CurrentProject += Views.Count - 1;
            }

            NewCommad.WithCanExecute(() => !_workspace.ProjectFile.IsEmpty)
               .ToFlow(this.ShowDialog<IProjectNameDialog, NewProjectDialogResult, string>(_workspace.ProjectFile.Projects.Select(p => p.ProjectName)))
                    .To.Mutate(_workspace.Projects).For(pm => pm.NewProject, pm => result => pm.AddProject(result.Name)).ToSelf()
               .Then.Action(p => AddProject(p.Project))
               .Return().ThenRegister("AddNewProject");

            #endregion
        }

        private void TryRemoveProject()
        {
            var currentProject = CurrentProject.Value;
            if (currentProject == null) return;

            var (_, projectName, _, _) = Views[currentProject.Value].Project;

            UICall(async c =>
                   {
                       var result = await _dialogCoordinator.ShowMessageAsync("MainWindow", string.Format(_localizer.CenterViewRemoveProjectDialogTitle, projectName),
                           _localizer.CenterViewRemoveProjectDialogMessage, MessageDialogStyle.AffirmativeAndNegative);
                       if (result == MessageDialogResult.Negative) return;

                       _workspace.Projects.RemoveProject(projectName);
                   });
        }


        private string GetActorName(string projectName) => projectName.Replace(' ', '_') + "-View";

        private void NewProjectDialogResult(NewProjectDialogResult obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Name)) return;

            _workspace.Projects.AddProject(obj.Name);
        }

        private void RemoveProject(Project project)
        {
            var proj = Views.FirstOrDefault(p => p.Project.ProjectName == project.ProjectName);
            if(proj == null) return;

            Context.Stop(Context.Child(GetActorName(proj.Project.ProjectName)));
            Views.Remove(proj);
        }

        private void ProjectSaved(SavedProject obj)
        {
            var controller = _manager.Find(obj.OperationId);
            if(controller == null) return;

            if (obj.Ok)
            {
                _mainWindow.Saved = true;
                controller.Compled();
            }
            else
            {
                _mainWindow.Saved = false;
                controller.Failed(obj.Exception?.Message);
            }
        }

        private void SaveRequested(SaveRequest obj)
        {
            if(string.IsNullOrWhiteSpace(obj.ProjectFile.Source)) return;

            var operation = _manager.StartOperation(string.Format(_localizer.CenterViewSaveProjectOperation, Path.GetFileName(obj.ProjectFile.Source)));
            var file = obj.ProjectFile;
            file.Operator.Tell(new SaveProject(operation.Id, file), Self);
        }
        
        private void UpdateSourceHandler(UpdateSource obj) => _workspace.Source.UpdateSource(obj.Name);

    }
}