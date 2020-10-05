using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Tauron.Application.Localizer.DataModel;
using Tauron.Application.Wpf;
using Tauron.Application.Wpf.Commands;

namespace Tauron.Application.Localizer.UIModels
{
    public sealed class ProjectEntryModel : ObservableObject
    {
        private readonly string _projectName;
        private readonly Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> _updater;

        public ProjectEntryModel(Project project, LocEntry target, Action<(string ProjectName, string EntryName, ActiveLanguage Lang, string Content)> updater,
            Action<(string ProjectName, string EntryName)> remove)
        {
            _updater = updater;
            _projectName = project.ProjectName;
            EntryName = target.Key;
            Entries = new UIObservableCollection<ProjectLangEntry>();
            RemoveCommand = new SimpleCommand(() => remove((_projectName, EntryName)));
            CopyCommand = new SimpleCommand(() => Clipboard.SetText(EntryName));

            foreach (var language in project.ActiveLanguages)
            {
                Entries.Add(target.Values.TryGetValue(language, out var content)
                    ? new ProjectLangEntry(EntryChanged, language, content)
                    : new ProjectLangEntry(EntryChanged, language, string.Empty));
            }
        }

        public string EntryName { get; }

        public UIObservableCollection<ProjectLangEntry> Entries { get; }

        public ICommand CopyCommand { get; }

        public ICommand RemoveCommand { get; }

        private void EntryChanged(string content, ActiveLanguage language)
        {
            _updater((_projectName, EntryName, language, content));
        }

        public void AddLanguage(ActiveLanguage lang)
        {
            Entries.Add(new ProjectLangEntry(EntryChanged, lang, string.Empty));
        }

        public void Update(LocEntry entry)
        {
            foreach (var (activeLanguage, content) in entry.Values)
            {
                var ent = Entries.FirstOrDefault(f => f.Language == activeLanguage);
                if (ent == null)
                    Entries.Add(new ProjectLangEntry(EntryChanged, activeLanguage, content));
                else
                    ent.UpdateContent(content);
            }
        }
    }
}