using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Threading;
using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Localizer.DataModel.Workspace;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating;
using Tauron.Application.Localizer.UIModels.Core;
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutation;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Dialogs;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private string _project = string.Empty;

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            #region Init

            var loadTrigger = new CommandTrigger();

            Receive<IncommingEvent>(e => e.Action());

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(!workspace.ProjectFile.IsEmpty);

            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys))
                .AndAsync();
            SelectedIndex = RegisterProperty<int>(nameof(SelectedIndex));

            var self = Context.Self;

            void TryUpdateEntry((string ProjectName, string EntryName, ActiveLanguage Lang, string Content) data)
            {
                var (projectName, entryName, lang, content) = data;
                self.Tell(new UpdateRequest(entryName, lang, content, projectName));
            }


            void TryRemoveEntry((string ProjectName, string EntryName) data)
            {
                var (projectName, entryName) = data;
                self.Tell(new RemoveRequest(entryName, projectName));
            }

            OnPreRestart += (exception, o) => Self.Tell(new InitProjectViewModel(workspace.Get(_project)));

            void InitProjectViewModel(InitProjectViewModel obj)
            {
                _project = obj.Project.ProjectName;

                Languages!.Add(new ProjectViewLanguageModel(localizer.ProjectViewLanguageBoxFirstLabel, true));
                Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
                SelectedIndex += 0;

                foreach (var projectEntry in obj.Project.Entries.OrderBy(le => le.Key)) 
                    ProjectEntrys.Add(new ProjectEntryModel(obj.Project, projectEntry, TryUpdateEntry, TryRemoveEntry));

                ImportetProjects!.AddRange(obj.Project.Imports);
                loadTrigger.Trigger();
            }

            Receive<InitProjectViewModel>(InitProjectViewModel);

            #endregion

            #region New Entry

            IEnumerable<NewEntryInfoBase> GetEntrys()
            {
                var list = ImportetProjects.ToList();
                list.Add(_project);

                var allEntrys = list.SelectMany(pro => workspace.Get(pro).Entries.Select(e => e.Key)).ToArray();

                return allEntrys.Select(e => new NewEntryInfo(e)).OfType<NewEntryInfoBase>()
                    .Concat(allEntrys
                        .Select(s => s.Split('_', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct(StringComparer.Ordinal)
                        .Select(s => new NewEntrySuggestInfo(s!)));

            }

            void AddEntry(EntryAdd entry)
            {
                if(_project != entry.Entry.Project) return;

                ProjectEntrys.Add(new ProjectEntryModel(workspace.Get(_project), entry.Entry, TryUpdateEntry, TryRemoveEntry));
            }

            NewCommad
               .ThenFlow(this.ShowDialog<INewEntryDialog, NewEntryDialogResult?, IEnumerable<NewEntryInfoBase>>(GetEntrys),
                    b =>
                    {
                        b.Mutate(workspace.Entrys).With(em => em.EntryAdd, em => res => em.NewEntry(_project, res!.Name)).ToSelf()
                           .Then(b2 => b2.Action(AddEntry));
                    })
               .ThenRegister("NewEntry");

            #endregion

            #region Remove Request

            void RemoveEntry(EntryRemove entry)
            {
                if (_project != entry.Entry.Project) return;

                var index = ProjectEntrys.FindIndex(em => em.EntryName == entry.Entry.Key);
                if (index == -1) return;

                ProjectEntrys.RemoveAt(index);
            }

            Flow<RemoveRequest>(b =>
            {
                b.Mutate(workspace.Entrys).With(em => em.EntryRemove, em => rr => em.RemoveEntry(rr.ProjectName, rr.EntryName)).ToSelf()
                   .Then(b2 => b2.Action(RemoveEntry));
            });

            #endregion

            #region Update Request

            void UpdateEntry(EntryUpdate obj)
            {
                if (_project != obj.Entry.Project) return;

                var model = ProjectEntrys.FirstOrDefault(m => m.EntryName == obj.Entry.Key);
                model?.Update(obj.Entry);
            }

            Flow<UpdateRequest>(b =>
            {
                b.Mutate(workspace.Entrys).With(
                        em => em.EntryUpdate,
                        em => ur => em.UpdateEntry(ur.ProjectName, ur.Language, ur.EntryName, ur.Content)).ToSelf()
                   .Then(b2 => b2.Action(UpdateEntry));
            });

            #endregion

            #region Imports

            void AddImport(AddImport obj)
            {
                if (obj.ProjectName != _project) return;
                ImportetProjects.Add(obj.Import);
            }

            IEnumerable<string> GetImportableProjects()
            {
                var pro = workspace.Get(_project);
                return workspace.ProjectFile.Projects.Select(p => p.ProjectName).Where(p => p != _project && !pro.Imports.Contains(p));
            }

            ImportSelectIndex = RegisterProperty<int>(nameof(ImportSelectIndex)).WithDefaultValue(-1);
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).AndAsync();

            NewCommad.WithCanExecute(b => new[]
                                          {
                                              b.FromEventSource(workspace.Projects.NewImport, _ => GetImportableProjects().Any(), null!),
                                              b.FromTrigger(() => GetImportableProjects().Any(), loadTrigger)
                                          })
               .ThenFlow(
                    this.ShowDialog<IImportProjectDialog, ImportProjectDialogResult?, IEnumerable<string>>(GetImportableProjects),
                    b =>
                    {
                        b.Mutate(workspace.Projects).With(pm => pm.NewImport, pm => r => pm.AddImport(_project, r!.Project)).ToSelf()
                           .Then(b1 => b1.Action(AddImport));
                    })
               .ThenRegister("AddImport");

            void RemoveImport(RemoveImport import)
            {
                if (_project != import.TargetProject) return;

                ImportetProjects.Remove(import.ToRemove);
            }

            NewCommad.WithCanExecute(b => b.FromProperty(ImportSelectIndex, i => i != -1))
               .ThenFlow(
                    () => new InitImportRemove(ImportetProjects[ImportSelectIndex]),
                    b =>
                    {
                        b.Mutate(workspace.Projects).With(pm => pm.RemoveImport, pm => ir => pm.TryRemoveImport(_project, ir.ToRemove)).ToSelf()
                           .Then(b1 => b1.Action(RemoveImport));
                    })
               .ThenRegister("RemoveImport");

            #endregion

            #region AddLanguage

            void AddActiveLanguage(AddActiveLanguage language)
            {
                if (language.ProjectName != _project) return;

                Languages.Add(new ProjectViewLanguageModel(language.ActiveLanguage.Name, false));

                foreach (var model in ProjectEntrys)
                    model.AddLanguage(language.ActiveLanguage);
            }


            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).AndAsync();

            NewCommad
                .ThenFlow(
                    this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, IEnumerable<CultureInfo>>(
                    () => workspace.Get(_project).ActiveLanguages.Select(al => al.ToCulture()).ToArray()),
                    b =>
                    {
                        b.Mutate(workspace.Projects).With(pm => pm.NewLanguage, pm => d => pm.AddLanguage(_project, d!.CultureInfo)).ToSelf()
                           .Then(b1 => b1.Action(AddActiveLanguage));
                    })
               .ThenRegister("AddLanguage");

            #endregion
        }

        public UIProperty<bool> IsEnabled { get; }

        public UICollectionProperty<ProjectViewLanguageModel> Languages { get; }
        public UIProperty<int> SelectedIndex { get; set; }
        public UICollectionProperty<ProjectEntryModel> ProjectEntrys { get; }

        public UIProperty<int> ImportSelectIndex { get; }

        public UICollectionProperty<string> ImportetProjects { get; }

        private sealed class InitImportRemove
        {
            public InitImportRemove(string remove)
            {
                ToRemove = remove;
            }

            public string ToRemove { get; }
        }

        private sealed class UpdateRequest
        {
            public UpdateRequest(string entryName, ActiveLanguage language, string content, string projectName)
            {
                EntryName = entryName;
                Language = language;
                Content = content;
                ProjectName = projectName;
            }

            public string EntryName { get; }

            public ActiveLanguage Language { get; }

            public string Content { get; }

            public string ProjectName { get; }
        }

        private sealed class RemoveRequest
        {
            public RemoveRequest(string entryName, string projectName)
            {
                EntryName = entryName;
                ProjectName = projectName;
            }

            public string EntryName { get; }

            public string ProjectName { get; }
        }
    }
}