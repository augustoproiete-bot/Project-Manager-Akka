using System.Collections.Generic;
using System.Linq;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.MutatingEngine;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public class EntryMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public EntryMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;

            EntryRemove = engine.EventSource(context => new EntryRemove(context.GetChange<RemoveEntryChange>().Entry), context => context.Change is RemoveEntryChange);
            EntryUpdate = engine.EventSource(context => new EntryUpdate(context.GetChange<EntryChange>().Entry), context => context.Change is EntryChange);
        }

        public IEventSource<EntryRemove> EntryRemove { get; }

        public IEventSource<EntryUpdate> EntryUpdate { get; }

        public void RemoveEntry(string project, string name)
        {
            _engine.Mutate(nameof(RemoveEntry), context =>
                                                {
                                                    var entry = context.Data.Projects.FirstOrDefault(p => p.ProjectName == project)?.Entries.Find(le => le.Key == name);
                                                    return entry == null ? context : context.Update(new RemoveEntryChange(entry), context.Data.ReplaceEntry(entry, null));
                                                });
        }

        public void UpdateEntry(string project, ActiveLanguage lang, string name, string content)
        {
            _engine.Mutate(nameof(UpdateEntry), context =>
                                                {
                                                    var entry = context.Data.Projects.FirstOrDefault(p => p.ProjectName == project)?.Entries.Find(le => le.Key == name);

                                                    if (entry == null) return context;
                                                    var oldContent = entry.Values.GetValueOrDefault(lang);
                                                    var newEntry = entry.WithValues(oldContent == null ? entry.Values.Add(lang, content) : entry.Values.SetItem(lang, content));

                                                    return context.Update(new EntryChange(newEntry), context.Data.ReplaceEntry(entry, newEntry));
                                                });
        }
    }
}