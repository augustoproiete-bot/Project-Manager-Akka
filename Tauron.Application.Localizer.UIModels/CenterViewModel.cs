using System;
using System.Linq;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Messages;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Localizer.UIModels.Services.Data;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [PublicAPI]
    public sealed class CenterViewModel : UiActor
    {
        private readonly IOperationManager _manager;
        private readonly LocLocalizer _localizer;
        private readonly ProjectFileWorkspace _workspace;

        private ViewCollection Views
        {
            get => Get<ViewCollection>()!;
            set => Set(value);
        }

        public CenterViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, IOperationManager manager, LocLocalizer localizer) 
            : base(lifetimeScope, dispatcher)
        {
            Views = new ViewCollection();

            _manager = manager;
            _localizer = localizer;
            _workspace = new ProjectFileWorkspace(Context);

            RegisterCommand("AddNewProject", TryAddProject, () => !_workspace.ProjectFile.IsEmpty);

            _workspace.Source.SaveRequest.RespondOn(Self);
            _workspace.Source.ProjectReset.RespondOn(Self);

            Receive<UpdateSource>(UpdateSourceHandler);
            Receive<SupplyNewProjectFile>(LoadNewProject);

            Receive<SaveRequest>(SaveRequested);
            Receive<SavedProject>(ProjectSaved);
            Receive<ProjectRest>(ProjectRest);
        }

        private void AddProject(Project project)
        {
            //TODO Add Project
        }

        private void TryAddProject()
        {

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
            var operation = _manager.StartOperation(_localizer.CenterViewSaveProjectOperation);
            var file = obj.ProjectFile;
            file.Operator.Tell(new SaveProject(operation.Id, file), Self);
        }

        private void LoadNewProject(SupplyNewProjectFile obj) => _workspace.Source.Reset(obj.File);

        private void UpdateSourceHandler(UpdateSource obj) => _workspace.Source.UpdateSource(obj.Name);

        private sealed class ViewContainer
        {
            [UsedImplicitly]
            public IViewModel Model { get; }

            public Project Project { get; }

            public ViewContainer(IViewModel model, Project project)
            {
                Model = model;
                Project = project;
            }
        }

        private sealed class ViewCollection : UIObservableCollection<ViewContainer>
        {
            
        }
    }
}