using System;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.UIModels.Core;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Services;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Commands;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectBuildpathRequest
    {
        public string TargetPath { get; }

        public string Project { get; }

        public ProjectBuildpathRequest(string targetPath, string project)
        {
            TargetPath = targetPath;
            Project = project;
        }
    }

    public sealed class BuildProjectViewModel : ObservableObject
    {
        private readonly EnterFlow<ProjectBuildpathRequest> _updatePath;
        private string _path;
        private readonly LocLocalizer _localizer;
        private readonly IDialogFactory _dialogFactory;
        private string _source;

        public string Project { get; }

        public ICommand Search { get; }

        public string Label { get; }

        public string Path
        {
            get => _path;
            set
            {
                if (value == _path) return;
                _path = value;
                OnPropertyChanged();

                _updatePath(new ProjectBuildpathRequest(value, Project));
            }
        }

        public BuildProjectViewModel(EnterFlow<ProjectBuildpathRequest> updatePath, string? path, string project, LocLocalizer localizer, IDialogFactory dialogFactory, string source)
        {
            _path = path ?? string.Empty;
            Project = project;
            _localizer = localizer;
            _dialogFactory = dialogFactory;

            source = source.GetDirectoryName();
            _source = string.IsNullOrWhiteSpace(source) ? Environment.CurrentDirectory : source;
            _updatePath = updatePath;

            Label = string.Format(localizer.MainWindowBuildProjectLabel, project);
            Search = new SimpleCommand(SetPathDialog);
        }

        private void SetPathDialog()
        {
            var path = _dialogFactory.ShowOpenFolderDialog(null, 
                _localizer.MainWindowBuildProjectFolderBrowserDescription, _source, 
                true, false, out var isOk);

            if(isOk != true) return;
            
            if(string.IsNullOrWhiteSpace(path)) return;

            Path = BuildViewModel.MakeRelativePath(path.CombinePath("lang"), _source);
        }

        public void UpdateSource(string newSource)
        {
            var compledPath = _path.CombinePath(_source);
            var newPath = BuildViewModel.MakeRelativePath(compledPath, newSource);
            _source = newSource;
            Path = newPath;
        }
    }

    [UsedImplicitly]
    public sealed class BuildViewModel : UiActor
    {
        public BuildViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, ProjectFileWorkspace workspace, LocLocalizer localizer, IDialogFactory dialogFactory, IOperationManager manager) 
            : base(lifetimeScope, dispatcher)
        {
            Receive<IncommingEvent>(e => e.Action());

            #region Import Integration

            Importintegration = RegisterProperty<bool>(nameof(Importintegration))
               .WithDefaultValue(true).ThenFlow(b => new ChangeIntigrate(b), this, b =>
                {
                    b.Mutate(workspace.Build).With(bm => bm.Intigrate, bm => ci => bm.SetIntigrate(ci.ToIntigrate)).ToSelf()
                       .Then(b2 => b2.Action(ii => Importintegration!.Set(ii.IsIntigrated)));
                });
            
            #endregion

            #region Projects

            Projects = this.RegisterUiCollection<BuildProjectViewModel>(nameof(Projects)).AndAsync();

            var flow = EnterFlow<ProjectBuildpathRequest>(b =>
            {
                b.Mutate(workspace.Build).With(bm => bm.ProjectPath, bm => r => bm.SetProjectPath(r.Project, r.TargetPath)).ToSelf()
                   .Then(b2 => b2.Action(UpdatePath).AndBuild());
            });

            BuildProjectViewModel GetBuildModel(Project p, ProjectFile file)
                => new BuildProjectViewModel(flow ?? throw new InvalidOperationException("Flow was null"), file.FindProjectPath(p), p.ProjectName, localizer, dialogFactory, file.Source);

            void InitProjects(ProjectRest rest)
            {
                if(flow == null) return;

                var file = rest.ProjectFile;

                Projects.Clear();
                Projects.AddRange(file.Projects.Select(p => GetBuildModel(p, file)));
            }

            void UpdatePath(ProjectPathChanged changed)
            {
                var model = Projects.FirstOrDefault(m => m.Project == changed.TargetProject);
                if(model == null) return;

                model.Path = changed.TargetPath;
            }

            this.RespondOnEventSource(workspace.Projects.NewProject, project => Projects.Add(GetBuildModel(project.Project, workspace.ProjectFile)));
            this.RespondOnEventSource(workspace.Projects.RemovedProject, project =>
            {
                var model = Projects.FirstOrDefault(m => m.Project == project.Project.ProjectName);
                if(model == null) return;

                Projects.Remove(model);
            });
            this.RespondOnEventSource(workspace.Source.SourceUpdate, updated => Projects.Foreach(m => m.UpdateSource(updated.Source)));

            #endregion

            #region Terminal

            TerminalMessages = this.RegisterUiCollection<string>(nameof(TerminalMessages));
            var buildMessageLocalizer = new BuildMessageLocalizer(localizer);

            Flow<BuildMessage>(b => b.Action(AddMessage));

            void ClearTerminal() => UICall(TerminalMessages.Clear);

            void AddMessage(BuildMessage message)
            {
                var locMsg = buildMessageLocalizer.Get(message);
                manager.Find(message.OperationId)?.UpdateStatus(locMsg);
                UICall(() => TerminalMessages.Add(locMsg));
            }

            #endregion

            #region Build

            var canBuild = QueryProperty.Create(true);

            this.RespondOnEventSource(workspace.Source.SaveRequest, _ => InvokeCommand("StartBuild"));

            NewCommad
               .WithCanExecute(b => new[]
                                    {
                                        b.FromProperty(canBuild),
                                        b.FromEventSource(workspace.Source.ProjectReset, _ => !workspace.ProjectFile.IsEmpty && !string.IsNullOrWhiteSpace(workspace.ProjectFile.Source))
                                    })
               .WithExecute(ClearTerminal)
               .WithExecute(() => canBuild.Value = false)
               .ThenFlow(() => new BuildRequest(manager.StartOperation(localizer.MainWindowodelBuildProjectOperation).Id, workspace.ProjectFile), b =>
                {
                    b.External<BuildCompled>(() => workspace.ProjectFile.Operator)
                       .Then(b2 => b2.Action(BuildCompled));
                })
               .ThenRegister("StartBuild");

            void BuildCompled(BuildCompled msg)
            {
                var op = manager.Find(msg.OperationId);
                if(op == null) return;

                if(msg.Failed)
                    op.Failed();
                else
                    op.Compled();

                canBuild!.Value = true;
            }

            #endregion

            #region Enable

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(false);
            this.RespondOnEventSource(workspace.Source.ProjectReset, r =>
            {
                IsEnabled += true;
                Importintegration += r.ProjectFile.BuildInfo.IntigrateProjects;
                InitProjects(r);
            });

            #endregion
        }

        public UIProperty<bool> IsEnabled { get; private set; }

        public UIProperty<bool> Importintegration { get; private set; }

        public UICollectionProperty<BuildProjectViewModel> Projects { get; }

        public UICollectionProperty<string> TerminalMessages { get; }

        public static string MakeRelativePath(string absolutePath, string pivotFolder)
        {
            //string folder = Path.IsPathRooted(pivotFolder)
            //    ? pivotFolder : Path.GetFullPath(pivotFolder);
            string folder = pivotFolder;
            Uri pathUri = new Uri(absolutePath);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            Uri relativeUri = folderUri.MakeRelativeUri(pathUri);
            return Uri.UnescapeDataString(
                relativeUri.ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        private sealed class BuildMessageLocalizer
        {
            private readonly LocLocalizer _localizer;

            public BuildMessageLocalizer(LocLocalizer localizer) => _localizer = localizer;

            public string Get(BuildMessage msg)
            {
                return msg.Message switch
                {
                    BuildMessage.Ids.AgentCompled => msg.Agent + _localizer.MainWindowBuildProjectAgentCompled,
                    BuildMessage.Ids.GenerateCsFiles => msg.Agent + _localizer.MainWindowBuildProjectGenerateCsFile,
                    BuildMessage.Ids.GenerateLangFiles => msg.Agent + _localizer.MainWindowBuildProjectGenerateLangFile,
                    BuildMessage.Ids.NoData => _localizer.MainWindowBuildprojectNoData,
                    BuildMessage.Ids.GatherData => msg.Agent + _localizer.MainWindowBuildProjectGatherData,
                    _ => msg.Message
                };
            }
        }

        private sealed class ChangeIntigrate
        {
            public bool ToIntigrate { get; }

            public ChangeIntigrate(bool intigrate) => ToIntigrate = intigrate;
        }
    }
}