using System.Collections.Generic;
using System.Linq;
using Functional.Maybe;
using Tauron.Application.Localizer.DataModel.Workspace.Mutating.Changes;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Workspace.Mutating
{
    public class EntryMutator
    {
        private readonly MutatingEngine<MutatingContext<ProjectFile>> _engine;

        public EntryMutator(MutatingEngine<MutatingContext<ProjectFile>> engine)
        {
            _engine = engine;

            EntryRemove = engine.EventSource(c => c.Select(cc => new EntryRemove(cc.GetChange<RemoveEntryChange>().Entry)),
                                             c => from constext in c
                                                  from change in constext.Change 
                                                  select change is RemoveEntryChange);

            EntryUpdate = engine.EventSource(c => c.Select(cc => new EntryUpdate(cc.GetChange<EntryChange>().Entry)),
                                             c => from context in c
                                                  from change in context.Change
                                                  select change is EntryChange);

            EntryAdd = engine.EventSource(c => c.Select(cc => cc.GetChange<NewEntryChange>().ToData()),
                                          c => from context in c
                                               from change in context.Change 
                                               select change is NewEntryChange);
        }

        public IEventSource<EntryRemove> EntryRemove { get; }

        public IEventSource<EntryUpdate> EntryUpdate { get; }

        public IEventSource<EntryAdd> EntryAdd { get; }

        public Maybe<Unit> RemoveEntry(string projectName, string name)
            => _engine.Mutate(nameof(RemoveEntry),
                              c =>
                                  from context in c
                                  from project in context.Data.Projects.FirstMaybe(p => p.ProjectName == projectName)
                                  from entry in MayNotNull(project.Entries.Find(le => le.Key == name)) 
                                  select context.WithChange(new RemoveEntryChange(entry)));


        public Maybe<Unit> UpdateEntry(string projectName, ActiveLanguage lang, string name, string content)
            => _engine.Mutate(nameof(UpdateEntry),
                              c =>
                                  from context in c
                                  from project in context.Data.Projects.FirstMaybe(p => p.ProjectName == projectName)
                                  from entry in project.Entries.FirstMaybe(e => e.Key == name)
                                  let newEntry = entry with{Values = entry.Values.SetItem(lang, content)}
                                  select context.WithChange(new EntryChange(entry, entry)));

        public Maybe<Unit> NewEntry(string projectName, string name)
            => _engine.Mutate(nameof(NewEntry),
                              c =>
                                  from context in c
                                  from project in context.Data.Projects.FirstMaybe(p => p.ProjectName == projectName)
                                  where project.Entries.All(e => e.Key != name)
                                  let newEntry = new LocEntry(projectName, name)
                                  select context.WithChange(new NewEntryChange(newEntry)));
    }
}