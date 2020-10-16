using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.Core;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Helper;
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
            Receive<IncommingEvent>(e => e.Action());

            Views = this.RegisterUiCollection<ProjectViewContainer>(nameof(Views)).AndAsync();
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

            Flow<SaveRequest>(b =>
            {
                b.EventSource(workspace.Source.SaveRequest).ToSelf()
                   .Then(b2 =>
                    {
                        b2.Action(SaveRequested)
                           .Then<SavedProject>(b3 => b3.Action(ProjectSaved));
                    });
            });

            #endregion

            #region Update Source

            Flow<UpdateSource>(b =>
            {
                b.Mutate(workspace.Source).With(sm => sm.SourceUpdate, sm => us => sm.UpdateSource(us.Name)).ToSelf()
                   .Then(b2 => b2.Action(su => mainWindow.TitlePostfix = Path.GetFileNameWithoutExtension(su.Source)));
            });

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

                Context.Stop(proj.Model.Actor);
                Views.Remove(proj);
            }

            NewCommad
               .WithCanExecute(b => new[]
                                    {
                                        b.FromProperty(CurrentProject, i => i != null),
                                        b.NoEmptyProjectFile(workspace)
                                    })
               .ThenFlow(TryGetRemoveProjectName, b =>
                {
                    b.Mutate(workspace.Projects).With(pm => pm.RemovedProject, pm => RemoveDialog).ToSelf()
                       .Then(b2 => b2.Action(rp => RemoveProject(rp.Project)));
                })
               .ThenRegister("RemoveProject");

            #endregion

            #region Project Reset

            OnPreRestart += (exception, msg) => Self.Tell(new ProjectRest(workspace.ProjectFile));
            OnPostStop += () =>
            {
                Views.Foreach(c => Context.Stop(c.Model.Actor));
                Thread.Sleep(1000);
            };

            void ProjectRest(ProjectRest obj)
            {
                mainWindow.Saved = File.Exists(obj.ProjectFile.Source);

                if (Views.Count != 0)
                {
                    Task.WhenAll(Views.Select(c => c.Model.Actor.GracefulStop(TimeSpan.FromMinutes(1)).ContinueWith(t => (c.Model.Actor, t))))
                       .ContinueWith(t =>
                        {
                            try
                            {
                                foreach (var (actor, stopped) in t.Result)
                                {
                                    try
                                    {
                                        if (stopped.Result)
                                            continue;
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Error(e, "Error on Stop Project Actor");
                                    }

                                    actor.Tell(Kill.Instance);
                                }
                            }
                            finally
                            {
                                Views.Clear();
                            }
                        }).PipeTo(Self, Sender, () => obj, _ => obj);

                    return;
                }

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

                mainWindow.IsBusy = false;
            }

            Flow<SupplyNewProjectFile>(b =>
            {
                b.Mutate(workspace.Source).With(sm => sm.ProjectReset, sm => np => sm.Reset(np.File)).ToSelf()
                   .Then(b2 => b2.Action(ProjectRest));
            });

            #endregion

            #region Add Project

            void AddProject(Project project)
            {
                string name = GetActorName(project.ProjectName);
                if (!ActorPath.IsValidPathElement(name))
                {
                    UICall(c => dialogCoordinator.ShowMessage(localizer.CommonError, localizer.CenterViewNewProjectInvalidNameMessage));
                    return;
                }

                var view = LifetimeScope.Resolve<IViewModel<ProjectViewModel>>();
                view.InitModel(Context, name);

                view.AwaitInit(() => view.Actor.Tell(new InitProjectViewModel(project), Self));
                Views.Add(new ProjectViewContainer(view, project));

                CurrentProject += Views.Count - 1;
            }

            NewCommad.WithCanExecute(b => b.NoEmptyProjectFile(workspace))
               .ThenFlow(
                    this.ShowDialog<IProjectNameDialog, NewProjectDialogResult, IEnumerable<string>>(() => workspace.ProjectFile.Projects.Select(p => p.ProjectName)),
                    b =>
                    {
                        b.Mutate(workspace.Projects).With(pm => pm.NewProject, pm => result => pm.AddProject(result.Name)).ToSelf()
                           .Then(b2 => b2.Action(p => AddProject(p.Project)));
                    })
               .ThenRegister("AddNewProject");

            #endregion

            #region Add Global Language

            NewCommad.WithCanExecute(b => b.NoEmptyProjectFile(workspace))
               .ThenFlow(
                    this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, IEnumerable<CultureInfo>>(() => workspace.ProjectFile.GlobalLanguages.Select(al => al.ToCulture())),
                    b =>
                    {
                        b.Mutate(workspace.Projects).With(mutator => result => mutator.AddLanguage(result?.CultureInfo));
                    })
               .ThenRegister("AddGlobalLang");

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