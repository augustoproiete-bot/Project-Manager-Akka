using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Actor;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Tauron.Application.Files.VirtualFiles;
using Tauron.Application.SoftwareRepo.Data;
using Tauron.Application.SoftwareRepo.Mutation;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.SoftwareRepo
{
    [PublicAPI]
    public sealed class SoftwareRepository : Workspace<SoftwareRepository, ApplicationList>
    {
        internal const string FileName = "Apps.json";

        public IDirectory Path { get; }

        public ApplicationList ApplicationList { get; private set; } = new ApplicationList();

        public IEventSource<ApplicationList> Changed { get; }

        public SoftwareRepository(IActorRefFactory factory, IDirectory path) 
            : base(new WorkspaceSuperviser(factory, "Software-Repository"))
        {
            Path = path;
            Changed = Engine.EventSource(mc => mc.GetChange<CommonChange>().ApplicationList, context => context.Change is CommonChange);
            Changed.RespondOn(null, Save);
        }


        internal void Init()
        {
            var file = GetFile();

            if (!file.Exist)
                throw new InvalidOperationException("Apps File not found");

            using var reader = new StreamReader(file.Open(FileAccess.Read));
            Reset(JsonConvert.DeserializeObject<ApplicationList>(reader.ReadToEnd()));
        }

        internal void InitNew()
        {
            var file = GetFile();

            if (!file.Exist)
                file.Delete();
            using var writer = new StreamWriter(file.CreateNew());
            writer.Write(JsonConvert.SerializeObject(ApplicationList));
        }

        private IFile GetFile() => Path.GetFile(FileName);

        private void Save(ApplicationList al)
        {
            using var writer = new StreamWriter(GetFile().CreateNew());
            writer.Write(JsonConvert.SerializeObject(al));
        }

        public void ChangeName(string? name = null, string? description = null)
        {
            Engine.Mutate(nameof(ChangeName), mc =>
                                              {
                                                  ApplicationList? newData = null;
                                                  if (!string.IsNullOrWhiteSpace(name))
                                                      newData = mc.Data.WithName(name);
                                                  if (!string.IsNullOrWhiteSpace(description))
                                                      newData = (newData ?? mc.Data).WithDescription(description);

                                                  return newData == null ? mc : mc.Update(new CommonChange(newData), newData);
                                              });
        }

        public long Get(string name) => ApplicationList.ApplicationEntries.Find(ae => ae.Name == name)?.Id ?? -1;

        public void AddApplication(string name, long id, string url, Version version, string originalRepository, string brnachName)
        {
            if (Get(name) != -1)
                return;

            Engine.Mutate(nameof(AddApplication),
                mc =>
                {
                    var newData = mc.Data.WithApplicationEntries(
                        mc.Data.ApplicationEntries.Add(
                            new ApplicationEntry(name, version, id, ImmutableList<DownloadEntry>.Empty.Add(new DownloadEntry(version, url)), originalRepository, brnachName)));

                    return mc.Update(new CommonChange(newData), newData);
                });
        }

        public void UpdateApplication(long id, Version version, string url)
        {
            Engine.Mutate(nameof(UpdateApplication),
                mc =>
                {
                    var entry = ApplicationList.ApplicationEntries.Find(ae => ae.Id == id);
                    if (entry == null)
                        return mc;

                    var newData = mc.Data.WithApplicationEntries(
                        mc.Data.ApplicationEntries.Replace(entry,
                            entry.WithLast(version).WithDownloads(entry.Downloads.Add(new DownloadEntry(version, url)))));

                    return mc.Update(new CommonChange(newData), newData);
                });
        }

        public void Save() => Save(ApplicationList);

        protected override MutatingContext<ApplicationList> GetDataInternal() 
            => MutatingContext<ApplicationList>.New(ApplicationList);

        protected override void SetDataInternal(MutatingContext<ApplicationList> data) 
            => ApplicationList = data.Data;
    }
}