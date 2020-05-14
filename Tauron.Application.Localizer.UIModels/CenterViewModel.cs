using System;
using System.Linq;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using MahApps.Metro.Controls.Dialogs;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Services.Data;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [PublicAPI]
    public sealed class CenterViewModel : UiActor
    {
        private readonly IOperationManager _manager;
        private readonly LocLocalizer _localizer;
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly ProjectFileWorkspace _workspace;

        private ProjectViewCollection Views
        {
            get => Get<ProjectViewCollection>()!;
            set => Set(value);
        }

        public CenterViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager manager, LocLocalizer localizer, IDialogCoordinator dialogCoordinator) 
            : base(lifetimeScope, dispatcher)
        {
            Views = new ProjectViewCollection();

            _manager = manager;
            _localizer = localizer;
            _dialogCoordinator = dialogCoordinator;
            _workspace = new ProjectFileWorkspace(Context);

            RegisterCommand("AddNewProject", TryAddProject, () => !_workspace.ProjectFile.IsEmpty);

            _workspace.Source.SaveRequest.RespondOn(Self);
            _workspace.Source.ProjectReset.RespondOn(Self);
            _workspace.Projects.NewProject.RespondOn(Self);

            Receive<UpdateSource>(UpdateSourceHandler);
            Receive<SupplyNewProjectFile>(LoadNewProject);

            Receive<SaveRequest>(SaveRequested);
            Receive<SavedProject>(ProjectSaved);
            Receive<ProjectRest>(ProjectRest);
            Receive<NewProjectDialogResult>(NewProjectDialogResult);
            Receive<AddProject>(ap => AddProject(ap.Project));
        }

        private void AddProject(Project project)
        {
            //TODO Add Project
        }


        private void NewProjectDialogResult(NewProjectDialogResult obj)
        {
            if(string.IsNullOrWhiteSpace(obj.Name))
            _workspace.Projects.AddProject(obj.Name);
        }

        private void TryAddProject()
        {
            var self = Self;
            Dispatcher.Invoke(async () =>
            {
                var diag = LifetimeScope.Resolve<IProjectNameDialog>();
                diag.Init(_workspace.ProjectFile.Projects.Select(p => p.ProjectName), self);

                await _dialogCoordinator.ShowMetroDialogAsync("MainWindow", diag.Dialog);
            });
        }

        private void RemoveProject(Project project)
        {
            var proj = Views.FirstOrDefault(p => p.Project.ProjectName == project.ProjectName);
            if(proj == null) return;

            Views.Remove(proj);
        }

        private void ProjectRest(ProjectRest obj)
        {
            Views.Clear();

            foreach (var project in obj.ProjectFile.Projects) 
                AddProject(project);

        }

        private void ProjectSaved(SavedProject obj)
        {
            var controller = _manager.Find(obj.OperationId);
            if(controller == null) return;

            if(obj.Ok)
                controller.Compled();
            else
                controller.Failed(obj.Exception?.Message);
        }

        private void SaveRequested(SaveRequest obj)
        {
            if(string.IsNullOrWhiteSpace(obj.ProjectFile.Source)) return;

            var operation = _manager.StartOperation(_localizer.CenterViewSaveProjectOperation);
            var file = obj.ProjectFile;
            file.Operator.Tell(new SaveProject(operation.Id, file), Self);
        }

        private void LoadNewProject(SupplyNewProjectFile obj) => _workspace.Source.Reset(obj.File);

        private void UpdateSourceHandler(UpdateSource obj) => _workspace.Source.UpdateSource(obj.Name);


    }
}