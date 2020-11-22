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

        public static ProjectFile FromSource(string maySource, IActorRef mayOp)
            => FromSource(May(maySource), May(mayOp)).Value;

        public static Maybe<ProjectFile> ReadFile(Maybe<BinaryReader> reader, Maybe<string> maySource, Maybe<IActorRef> mayOp) 
            => from source in maySource
               from op in mayOp
               let file = new ProjectFile(source, op)
               from vers in ReadInt32(reader)
               from projects in ReadList(reader, Project.ReadFrom)
               from globalLang in ReadList(reader, ActiveLanguage.ReadFrom)
               from buildInfo in vers == 2 ? BuildInfo.ReadFrom(reader) : May(new BuildInfo())
               select file with
                          {
                          Projects = projects,
                          GlobalLanguages = globalLang,
                          BuildInfo = buildInfo
                          };

        public static Maybe<Unit> BeginLoad(Maybe<IActorContext> mayFactory, Maybe<string> mayOperationId, Maybe<string> maySource, Maybe<string> mayActorName) 
            => from factory in mayFactory
               from source in maySource
               from operationId in mayOperationId
               from actor in factory.GetOrAdd<ProjectFileOperator>(mayActorName)
               select Action(() =>
                             {
                                 Tell(actor, new LoadProjectFile(operationId, source));
                                 Thread.Sleep(500);
                             });
    }
}