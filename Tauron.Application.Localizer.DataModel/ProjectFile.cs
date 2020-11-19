using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using Akka.Actor;
using Functional.Maybe;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.Localizer.DataModel.Processing;
using Tauron.Application.Localizer.DataModel.Serialization;
using static Tauron.Prelude;
using static Tauron.Application.Localizer.DataModel.Serialization.BinaryHelper;

namespace Tauron.Application.Localizer.DataModel
{
    [PublicAPI]
    public sealed partial record ProjectFile(ImmutableList<Project> Projects, ImmutableList<ActiveLanguage> GlobalLanguages, string Source, Maybe<IActorRef> Operator, BuildInfo BuildInfo) : IWriteable
    {
        public const int Version = 2;

        public ProjectFile()
            : this(ImmutableList<Project>.Empty, ImmutableList<ActiveLanguage>.Empty, string.Empty, Maybe<IActorRef>.Nothing, new BuildInfo())
        { }

        private ProjectFile(string source, IActorRef op)
            : this()
        {
            Source = source;
            Operator = MayActor(op);
        }

        public bool IsEmpty => Operator.IsNothing();

        public Maybe<Unit> WriteData(Maybe<BinaryWriter> writer) 
            => from v in Write(writer, Version) 
                from p in WriteList(writer, Projects) 
                from l in WriteList(writer, GlobalLanguages) 
                from i in BuildInfo.WriteData(writer) 
                select i;

        public static Maybe<ProjectFile> FromSource(Maybe<string> maySource, Maybe<IActorRef> mayOp)
            => from source in maySource
                from op in mayOp
                select new ProjectFile(source, op);

        public static Maybe<ProjectFile> ReadFile(Maybe<BinaryReader> reader, Maybe<string> source, Maybe<IActorRef> op)
        {
            var file = new ProjectFile(source, op);
            var builder = file.ToBuilder();

            var vers = reader.ReadInt32();
            builder.Projects = BinaryHelper.ReadList(reader, Project.ReadFrom);
            builder.GlobalLanguages = BinaryHelper.ReadList(reader, ActiveLanguage.ReadFrom);
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