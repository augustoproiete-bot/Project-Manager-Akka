using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Akka.Actor;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Serialization;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class ProjectFile : IWriteable
    {
        public const int Version = 2;

        public ProjectFile()
        {
            Projects = ImmutableList<Project>.Empty;
            Source = string.Empty;
            Operator = ActorRefs.Nobody;
            GlobalLanguages = ImmutableList<ActiveLanguage>.Empty;
            BuildInfo = new BuildInfo();
        }

        private ProjectFile(string source, IActorRef op)
            : this()
        {
            Source = source;
            Operator = op;
        }

        public ImmutableList<Project> Projects { get; }

        public ImmutableList<ActiveLanguage> GlobalLanguages { get; }

        public string Source { get; }

        public IActorRef Operator { get; }

        public bool IsEmpty => Operator.Equals(ActorRefs.Nobody);

        public BuildInfo BuildInfo { get; }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Version);
            BinaryHelper.WriteList(Projects, writer);
            BinaryHelper.WriteList(GlobalLanguages, writer);
            BuildInfo.Write(writer);
        }

        public static ProjectFile FromSource(string source, IActorRef op) 
            => new ProjectFile(source, op);

        public static ProjectFile ReadFile(BinaryReader reader, string source, IActorRef op)
        {
            var file = new ProjectFile(source, op);
            var builder = file.ToBuilder();

            var vers = reader.ReadInt32();
            builder.Projects = BinaryHelper.Read(reader, Project.ReadFrom);
            builder.GlobalLanguages = BinaryHelper.Read(reader, ActiveLanguage.ReadFrom);
            builder.BuildInfo = vers == 1 ? new BuildInfo() : BuildInfo.ReadFrom(reader);

            return builder.ToImmutable();
        }

        public static void BeginLoad(IActorContext factory, string operationId, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            actor.Tell(new LoadProjectFile(operationId, source));
            Thread.Sleep(500);
        }
    }
}