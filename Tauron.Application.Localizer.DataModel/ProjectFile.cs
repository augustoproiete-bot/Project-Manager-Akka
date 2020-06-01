using System;
using System.Collections.Immutable;
using System.IO;
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
        public ImmutableList<Project> Projects { get; }

        public ImmutableList<ActiveLanguage> GlobalLanguages { get; }

        public string Source { get; }

        public IActorRef Operator { get; }

        public bool IsEmpty => Operator.Equals(ActorRefs.Nobody);

        public ProjectFile()
        {
            Projects = ImmutableList<Project>.Empty;
            Source = string.Empty;
            Operator = ActorRefs.Nobody;
            GlobalLanguages = ImmutableList<ActiveLanguage>.Empty;
        }

        private ProjectFile(string source, IActorRef op)
            : this()
        {
            Source = source;
            Operator = op;
        }

        public static ProjectFile FromSource(string source, IActorRef op)
        {
            return new ProjectFile(source, op);
        }
        
        public static ProjectFile ReadFile(BinaryReader reader, string source, IActorRef op)
        {
            var file = new ProjectFile(source, op);
            var builder = file.ToBuilder();

            builder.Projects = Helper.Read(reader, Project.ReadFrom);
            builder.GlobalLanguages = Helper.Read(reader, ActiveLanguage.ReadFrom);

            return builder.ToImmutable();
        }

        public void Write(BinaryWriter writer)
        {
            Helper.WriteList(Projects, writer);
            Helper.WriteList(GlobalLanguages, writer);
        }

        public static void BeginLoad(IActorContext factory, string operationId, string source, string actorName)
        {
            var actor = factory.GetOrAdd<ProjectFileOperator>(actorName);
            actor.Tell(new LoadProjectFile(operationId, source));
        }
    }
}