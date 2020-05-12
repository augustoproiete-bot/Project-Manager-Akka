using System;
using System.Collections.Immutable;
using System.IO;
using Akka.Actor;
using Amadevus.RecordGenerator;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;

namespace Tauron.Application.Localizer.DataModel
{
    [Record]
    [PublicAPI]
    public sealed partial class ProjectFile
    {
        public ImmutableList<Project> Projects { get; }

        public string Source { get; }

        public IActorRef Operator { get; }

        public ProjectFile(BinaryReader reader, string source, IActorRef op)
        {
            Source = source;
            Operator = op;

            var count = reader.ReadInt32();
            var builder = ImmutableList<Project>.Empty.ToBuilder();

            for (var i = 0; i < count; i++)
                builder.Add(new Project(reader));

            Projects = builder.ToImmutable();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Projects.Count);
            foreach (var project in Projects) 
                project.Write(writer);
        }

        public static string BeginLoad(IActorContext factory, string source, string? name = null)
        {
            string opId = Guid.NewGuid().ToString();
            var actor = factory.GetOrAdd<ProjectFileOperator>(name);
            actor.Tell(new LoadProjectFile(opId, source));
            return opId;
        }
    }
}