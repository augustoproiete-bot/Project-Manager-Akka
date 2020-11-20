using Akka.Actor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Functional.Maybe;
using Tauron.Akka;
using Tauron.Localization;
using static Tauron.Prelude;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class BuildAgent : ExposedReceiveActor
    {
        public BuildAgent() 
            => Flow<PreparedBuild>(b => b.Func(OnBuild).Forward.ToParent());

        private Maybe<AgentCompled> OnBuild(Maybe<PreparedBuild> mayBuild)
        {
            var agentName = Context.Self.Path.Name + ": ";

            return MatchMay(
                from build in mayBuild
                select Try(() =>
                    from step1 in MayTell(Context.Sender, BuildMessage.GatherData(build.Operation, agentName))
                    from data in GetData(build)
                    
                    from step2 in MayTell(Context.Sender, BuildMessage.GenerateLangFiles(build.Operation, agentName))
                    from a in GenerateJson(data, build.TargetPath)

                    from step3 in MayTell(Context.Sender, BuildMessage.GenerateCsFiles(build.Operation, agentName))
                    from b in GenerateCode(data, build.TargetPath)

                    from final in MayTell(Context.Sender, BuildMessage.AgentCompled(build.Operation, agentName)) 
                    select new AgentCompled(false, Maybe<Exception>.Nothing, build.Operation)),
                e => May(new AgentCompled(true, May(e), mayBuild.Value.Operation)));
        }

        private static Maybe<Dictionary<string, GroupDictionary<string, (string Id, string Content)>>> GetData(PreparedBuild build)
        {
            var files = new Dictionary<string, GroupDictionary<string, (string Id, string Content)>>();

            var (buildInfo, targetProject, projectFile, _, _) = build;

            var imports = new List<(string FileName, Project Project)>
                          {
                              (string.Empty, targetProject)
                          };
            imports.AddRange(targetProject.Imports
               .Select(pn => projectFile.Projects.Find(p => p.ProjectName == pn))
               .Where(p => p != null)
               .Select(p => buildInfo.IntigrateProjects ? (string.Empty, p) : (p.ProjectName, p))!);

            foreach (var (fileName, project) in imports)
            {
                if (!files.TryGetValue(fileName, out var entrys))
                {
                    entrys = new GroupDictionary<string, (string Id, string Content)>();
                    files[fileName] = entrys;
                }

                foreach (var (_, id, values) in project.Entries)
                {
                    foreach (var ((shortcut, _), value) in values)
                        entrys.Add(shortcut, (id, value));
                }
            }

            return May(files);
        }

        private static Maybe<Unit> GenerateJson(Dictionary<string, GroupDictionary<string, (string Id, string Content)>> data, string targetPath)
        {
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            foreach (var (key, value) in data)
            {
                foreach (var (lang, entrys) in value)
                {
                    var fileName = (string.IsNullOrWhiteSpace(key) ? string.Empty : key + ".") + lang + ".json";
                    using var writer = new StreamWriter(File.Open(targetPath.CombinePath(fileName), FileMode.Create));
                    var tester = new HashSet<string>();
                    writer.WriteLine("{");

                    foreach (var (id, content) in entrys)
                    {
                        if(tester.Add(id))
                            writer.WriteLine($"  \"{id}\": \"{EscapeHelper.Ecode(content)}\",");
                    }

                    writer.WriteLine("}");
                    writer.Flush();
                }
            }

            return Unit.MayInstance;
        }

        private static Maybe<Unit> GenerateCode(Dictionary<string, GroupDictionary<string, (string Id, string Content)>> data, string targetPath)
        {
            var ids = new HashSet<string>(data
               .SelectMany(e => e.Value)
               .SelectMany(e => e.Value)
               .Select(e => e.Id));

            var entrys = new GroupDictionary<string, (string FieldName, string Original)>
                         {
                             string.Empty
                         };

            foreach (var id in ids)
            {
                var result = id.Split('_', 2, StringSplitOptions.RemoveEmptyEntries);
                if (result.Length == 1)
                    entrys.Add(string.Empty, (result[0].Replace("_", ""), id));
                else
                    entrys.Add(result[0], (result[1].Replace("_", ""), id));
            }

            using var file = new StreamWriter(File.Open(targetPath.CombinePath("LocLocalizer.cs"), FileMode.Create));

            file.WriteLine("using System.CodeDom.Compiler;");
            file.WriteLine("using System.Threading.Tasks;");
            file.WriteLine("using Akka.Actor;");
            file.WriteLine("using JetBrains.Annotations;");
            file.WriteLine("using Tauron.Localization;");
            file.WriteLine();
            file.WriteLine("namespace Tauron.Application.Localizer.Generated");
            file.WriteLine("{");
            file.WriteLine("\t[PublicAPI, GeneratedCode(\"Localizer\", \"1\")]");
            file.WriteLine("\tpublic sealed partial class LocLocalizer");
            file.WriteLine("\t{");

            var classes = new List<string>();

            foreach (var (key, value) in entrys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                classes.Add(key);

                file.WriteLine($"\t\tpublic sealed class {key}Res");
                file.WriteLine("\t\t{");
                WriteClassData(file, key + "Res", value, 3);
                file.WriteLine("\t\t}");
            }

            WriteClassData(file, "LocLocalizer", entrys[string.Empty], 2,
                writer =>
                {
                    foreach (var @class in classes)
                        writer(@class + " = new " + @class + "Res" + "(system);");
                });

            foreach (var @class in classes)
                file.WriteLine("\t\tpublic " + @class + "Res " + @class + " { get; }");

            file.WriteLine("\t\tprivate static Task<string> ToString(Task<object?> task)");
            file.WriteLine("\t\t\t=> task.ContinueWith(t => t.Result as string ?? string.Empty);");

            file.WriteLine("\t}");
            file.WriteLine("}");

            file.Flush();

            return Unit.MayInstance;
        }

        private static void WriteClassData(StreamWriter writer, string className, ICollection<(string FieldName, string Original)> entryValue, int tabs, 
            Action<Action<string>>? constructorCallback = null)
        {
            foreach (var (fieldName, _) in entryValue)
                writer.WriteLine($"{new string('\t', tabs)}private readonly Task<string> {ToRealFieldName(fieldName)};");

            writer.WriteLine($"{new string('\t', tabs)}public {className}(ActorSystem system)");
            writer.WriteLine(new string('\t', tabs) + "{");
            tabs++;
            writer.WriteLine($"{new string('\t', tabs)}var loc = system.Loc();");

            // ReSharper disable once AccessToModifiedClosure
            constructorCallback?.Invoke(s => writer.WriteLine($"{new string('\t', tabs)} {s}") );

            foreach (var (fieldName, original) in entryValue) 
                writer.WriteLine($"{new string('\t', tabs)}{ToRealFieldName(fieldName)} = LocLocalizer.ToString(loc.RequestTask(\"{original}\"));");

            tabs--;
            writer.WriteLine(new string('\t', tabs) + "}");

            foreach (var entry in entryValue) 
                writer.WriteLine($"{new string('\t', tabs)}public string {entry.FieldName} => {ToRealFieldName(entry.FieldName)}.Result;");
        }

        private static string ToRealFieldName(string name)
            => "__" + name;
    }
}