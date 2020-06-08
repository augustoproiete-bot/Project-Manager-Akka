﻿using System;
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
using Tauron.Application.Localizer.UIModels.lang;
using Tauron.Application.Localizer.UIModels.Views;
using Tauron.Application.Workshop;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels
{
    [UsedImplicitly]
    public sealed class ProjectViewModel : UiActor
    {
        private string _project = string.Empty;

        public ProjectViewModel(ILifetimeScope lifetimeScope, Dispatcher dispatcher, LocLocalizer localizer, IDialogCoordinator dialogCoordinator, ProjectFileWorkspace workspace)
            : base(lifetimeScope, dispatcher)
        {
            #region Init

            IsEnabled = RegisterProperty<bool>(nameof(IsEnabled)).WithDefaultValue(!workspace.ProjectFile.IsEmpty);

            ProjectEntrys = this.RegisterUiCollection<ProjectEntryModel>(nameof(ProjectEntrys)).Async();
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

            void InitProjectViewModel(InitProjectViewModel obj)
            {
                _project = obj.Project.ProjectName;

                Languages.Add(new ProjectViewLanguageModel(localizer.ProjectViewLanguageBoxFirstLabel, true));
                Languages.AddRange(obj.Project.ActiveLanguages.Select(al => new ProjectViewLanguageModel(al.Name, false)));
                SelectedIndex += 0;

                foreach (var projectEntry in obj.Project.Entries) 
                    ProjectEntrys.Add(new ProjectEntryModel(obj.Project, projectEntry, TryUpdateEntry, TryRemoveEntry));

                ImportetProjects.AddRange(obj.Project.Imports);
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
                        .Select(s => new NewEntrySuggestInfo(s)));

            }

            void AddEntry(EntryAdd entry)
            {
                if(_project != entry.Entry.Project) return;

                ProjectEntrys.Add(new ProjectEntryModel(workspace.Get(_project), entry.Entry, TryUpdateEntry, TryRemoveEntry));
            }

            NewCommad
                .ToFlow(this.ShowDialog<INewEntryDialog, NewEntryDialogResult?, NewEntryInfoBase>(GetEntrys))
                .To.Mutate(workspace.Entrys).For(em => em.EntryAdd, em => res => em.NewEntry(_project, res!.Name)).ToSelf()
                .Then.Action(AddEntry)
                .Return().ThenRegister("NewEntry");

            #endregion

            #region Remove Request

            void RemoveEntry(EntryRemove entry)
            {
                if (_project != entry.Entry.Project) return;

                var index = ProjectEntrys.FindIndex(em => em.EntryName == entry.Entry.Key);
                if (index == -1) return;

                ProjectEntrys.RemoveAt(index);
            }

            this.Flow<RemoveRequest>()
                .To.Mutate(workspace.Entrys).For(em => em.EntryRemove, em => rr => em.RemoveEntry(rr.ProjectName, rr.EntryName)).ToSelf()
                .Then.Action(RemoveEntry).Receive();

            #endregion

            #region Update Request

            void UpdateEntry(EntryUpdate obj)
            {
                if (_project != obj.Entry.Project) return;

                var model = ProjectEntrys.FirstOrDefault(m => m.EntryName == obj.Entry.Key);
                model.Update(obj.Entry);
            }

            this.Flow<UpdateRequest>()
                .To.Mutate(workspace.Entrys).For(em => em.EntryUpdate, em => ur => em.UpdateEntry(ur.ProjectName, ur.Language, ur.EntryName, ur.Content)).ToSelf()
                .Then.Action(UpdateEntry).Receive();

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
            ImportetProjects = this.RegisterUiCollection<string>(nameof(ImportetProjects)).Async();

            NewCommad.WithCanExecute(() => GetImportableProjects().Any())
                .ToFlow(this.ShowDialog<IImportProjectDialog, ImportProjectDialogResult?, string>(GetImportableProjects))
                .To.Mutate(workspace.Projects).For(pm => pm.NewImport, pm => r => pm.AddImport(_project, r!.Project)).ToSelf()
                .Then.Action(AddImport)
                .Return().ThenRegister("AddImport");

            void RemoveImport(RemoveImport import)
            {
                if (_project != import.TargetProject) return;

                ImportetProjects.Remove(import.ToRemove);
            }

            NewCommad.WithCanExecute(() => ImportSelectIndex.Value != -1)
                .ToFlow(() => new InitImportRemove(ImportetProjects[ImportSelectIndex]))
                .To.Mutate(workspace.Projects).For(pm => pm.RemoveImport, pm => ir => pm.TryRemoveImport(_project, ir.ToRemove)).ToSelf()
                .Then.Action(RemoveImport)
                .Return().ThenRegister("RemoveImport");

            #endregion

            #region AddLanguage

            void AddActiveLanguage(AddActiveLanguage language)
            {
                if (language.ProjectName != _project) return;

                Languages.Add(new ProjectViewLanguageModel(language.ActiveLanguage.Name, false));

                foreach (var model in ProjectEntrys)
                    model.AddLanguage(language.ActiveLanguage);
            }


            Languages = this.RegisterUiCollection<ProjectViewLanguageModel>(nameof(Languages)).Async();

            NewCommad
                .ToFlow(this.ShowDialog<ILanguageSelectorDialog, AddLanguageDialogResult?, CultureInfo>(() => workspace.Get(_project).ActiveLanguages.Select(al => al.ToCulture()).ToArray()))
                .To.Mutate(workspace.Projects).For(pm => pm.NewLanguage, pm => d => pm.AddLanguage(_project, d!.CultureInfo)).ToSelf()
                .Then.Action(AddActiveLanguage).Return().ThenRegister("AddLanguage");

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