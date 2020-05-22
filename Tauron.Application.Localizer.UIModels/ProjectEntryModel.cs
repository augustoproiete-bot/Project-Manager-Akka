﻿using System;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors.Core;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Wpf;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectEntryModel : ObservableObject
    {
        private readonly Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> _updater;
        private readonly string _projectName;

        public string EntryName { get; }

        public UIObservableCollection<ProjectLangEntry> Entries { get; }

        public ICommand RemoveCommand { get; }

        public ProjectEntryModel(Project project, LocEntry target, Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> updater, 
            Action<(string ProjectName, string EntryName)> remove)
        {
            _updater = updater;
            _projectName = project.ProjectName;
            EntryName = target.Key;
            Entries = new UIObservableCollection<ProjectLangEntry>();
            RemoveCommand = new ActionCommand(() => remove((_projectName, EntryName)));

            foreach (var language in project.ActiveLanguages)
            {
                Entries.Add(target.Values.TryGetValue(language, out var content) 
                    ? new ProjectLangEntry(EntryChanged, language, content) 
                    : new ProjectLangEntry(EntryChanged, language, string.Empty));
            }
        }

        private void EntryChanged(string content, ActiveLanguage language) 
            => _updater((_projectName, EntryName, language, content));

        public void AddLanguage(ActiveLanguage lang) 
            => Entries.Add(new ProjectLangEntry(EntryChanged, lang, string.Empty));
    }
}