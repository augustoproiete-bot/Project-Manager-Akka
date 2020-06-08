using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class CenterViewModel : UiActor
    {
        public CenterViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager manager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator,
            IMainWindowCoordinator mainWindow, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            Views = this.RegisterUiCollection<ProjectViewContainer>(nameof(Views)).Async();
            CurrentProject = RegisterProperty<int?>(nameof(CurrentProject));

            AddProject(new Project().WithProjectName("Dummy"));

            static string GetActorName(string projectName)
            {
                return projectName.Replace(' ', '_') + "-View";
            }

            #region Project Save

            void ProjectSaved(SavedProject obj)
            {
                var controller = manager.Find(obj.OperationId);
                if (controller == null) return;

                if (obj.Ok)
                {
                    mainWindow.Saved = true;
                    controller.Compled();
                }
                else
                {
                    mainWindow.Saved = false;
                    controller.Failed(obj.Exception?.Message);
                }
            }

            void SaveRequested(SaveRequest obj)
            {
                if (string.IsNullOrWhiteSpace(obj.ProjectFile.Source)) return;

                var operation = manager.StartOperation(string.Format(localizer.CenterViewSaveProjectOperation, Path.GetFileName(obj.ProjectFile.Source)));
                var file = obj.ProjectFile;
                file.Operator.Tell(new SaveProject(operation.Id, file), Self);
            }

            this.Flow<SaveRequest>().To.EventSource(workspace.Source.SaveRequest).ToSelf()
                .Then.Action(SaveRequested)
                .RespondTo<SavedProject>().Action(ProjectSaved).Receive();

            #endregion

            #region Update Source

            this.Flow<UpdateSource>().To.Mutate(workspace.Source).For(sm => sm.SourceUpdate, sm => us => sm.UpdateSource(us.Name)).ToSelf()
                .Then.Action(su => mainWindow.TitlePostfix = Path.GetFileNameWithoutExtension(su.Source)).Receive();

            #endregion

            #region Remove Project

            RemoveProjectName? TryGetRemoveProjectName()
            {
                var currentProject = CurrentProject.Value;
                if (currentProject == null) return null;

                var (_, projectName, _, _) = Views[currentProject.Value].Project;

                return new RemoveProjectName(projectName);
            }

            void RemoveDialog(RemoveProjectName? project)
            {
                UICall(c =>
                {
                    dialogCoordinator.ShowMessage(string.Format(localizer.CenterViewRemoveProjectDialogTitle, project?.Name),
                        localizer.CenterViewRemoveProjectDialogMessage, result =>
                        {
                            if (result == true)
                                workspace.Projects.RemoveProject(project?.Name ?? string.Empty);
                        });
                });
            }

            void RemoveProject(Project project)
            {
                var proj = Views.FirstOrDefault(p => p.Project.ProjectName == project.ProjectName);
                if (proj == null) return;

                Context.Stop(Context.Child(GetActorName(proj.Project.ProjectName)));
                Views.Remove(proj);
            }

            NewCommad.WithCanExecute(() => !workspace.ProjectFile.IsEmpty && CurrentProject != null)
                .ToFlow(TryGetRemoveProjectName()).To.Mutate(workspace.Projects).For(pm => pm.RemovedProject, pm => RemoveDialog).ToSelf()
                .Then.Action(rp => RemoveProject(rp.Project))
                .Return().ThenRegister("RemoveProject");

            #endregion

            #region Project Reset

            void ProjectRest(ProjectRest obj)
            {
                mainWindow.Saved = File.Exists(obj.ProjectFile.Source);

                //foreach (var view in Views)
                //    Context.Stop(view.Model.Actor);
                Views.Clear();

                string titleName = obj.ProjectFile.Source;
                if (string.IsNullOrWhiteSpace(titleName))
                {
                    titleName = localizer.CommonUnkowen;
                }
                else
                {
                    titleName = Path.GetFileNameWithoutExtension(obj.ProjectFile.Source);
                    if (string.IsNullOrWhiteSpace(titleName))
                        titleName = localizer.CommonUnkowen;
                }

                mainWindow.TitlePostfix = titleName;

                foreach (var project in obj.ProjectFile.Projects)
                    AddProject(project);

                CommandChanged();
                mainWindow.IsBusy = false;
            }

            this.Flow<SupplyNewProjectFile>().To.Mutate(workspace.Source).For(sm => sm.ProjectReset, sm => np => sm.Reset(np.File)).ToSelf()
                .Then.Action(ProjectRest).Receive();

            #endregion

            #region Add project

            void AddProject(Project project)
            {
                string name = GetActorName(project.ProjectName);
                if (!ActorPath.IsValidPathElement(name))
                {
                    UICall(c => dialogCoordinator.ShowMessage(localizer.CommonError, localizer.CenterViewNewProjectInvalidNameMessage));
                    return;
                }

                var view = LifetimeScope.Resolve<IViewModel<ProjectViewModel>>();
                view.Init(Context, name);
                view.Actor.Tell(new InitProjectViewModel(project), Self);

                Views.Add(new ProjectViewContainer(view, project));

                CurrentProject += Views.Count - 1;
            }

            NewCommad.WithCanExecute(() => !workspace.ProjectFile.IsEmpty)
                .ToFlow(this.ShowDialog<IProjectNameDialog, NewProjectDialogResult, string>(() => workspace.ProjectFile.Projects.Select(p => p.ProjectName)))
                .To.Mutate(workspace.Projects).For(pm => pm.NewProject, pm => result => pm.AddProject(result.Name)).ToSelf()
                .Then.Action(p => AddProject(p.Project))
                .Return().ThenRegister("AddNewProject");

            #endregion

            #region Add Global Language

            NewCommad.WithCanExecute(() => !workspace.ProjectFile.IsEmpty)
                .ToFlow(this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, CultureInfo>(() => workspace.ProjectFile.GlobalLanguages.Select(al => al.ToCulture())))
                .To.Mutate(workspace.Projects).For(mutator => result => mutator.AddLanguage(result?.CultureInfo))
                .Return().ThenRegister("AddGlobalLang");

            #endregion
        }

        private UICollectionProperty<ProjectViewContainer> Views { get; }

        private UIProperty<int?> CurrentProject { get; set; }

        private sealed class RemoveProjectName
        {
            public RemoveProjectName(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }
    }
}