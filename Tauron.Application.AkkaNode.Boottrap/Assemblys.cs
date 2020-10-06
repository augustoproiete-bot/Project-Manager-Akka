namespace Tauron.Application.AkkaNode.Bootstrap
{
    //[PublicAPI]
    //public static class Assemblys
    //{
    //    private static object _lock = new object();

    //    private static ImmutableList<string> _probePaths = ImmutableList<string>
    //       .Empty.AddRange(new []
    //                       {
    //                           "..\\SharedDll",
    //                           "..\\..\\SharedDll"
    //                       });

    //    private static ImmutableList<string> _assemblyList = ImmutableList<string>
    //       .Empty.AddRange(new []
    //                       {
    //                           "Akka.Cluster",
    //                           "Akka.Cluster.Sharding",
    //                           "Akka.Cluster.Tools",
    //                           "Akka.Cluster.Utility",
    //                           "Akka.DI.AutoFac",
    //                           "Akka.DI.Core",
    //                           "Akka.DistributedData",
    //                           "Akka.DistributedData.LightningDB",
    //                           "Akka",
    //                           "Akka.Logger.Serilog",
    //                           "Akka.Persistence",
    //                           "Akka.Persistence.Query",
    //                           "Akka.Remote",
    //                           "Akka.Streams",
    //                           "Akka.Coordination"
    //                       });

    //    public static IList<string> ProbePaths => _probePaths;

    //    public static IList<string> AssemblyList => _assemblyList;

    //    public static void WireUp()
    //    {
    //        AssemblyLoadContext.Default.Resolving += (context, name) =>
    //        {
    //            var simpleName = _assemblyList.FirstOrDefault(s => name.Name?.Contains(s) == true);
    //            if(string.IsNullOrWhiteSpace(simpleName))
    //                return null;

    //            simpleName += ".dll";
    //            return (from path in _probePaths 
    //                    select Path.GetFullPath(Path.Combine(path, simpleName)) 
    //                    into fullPath 
    //                    where File.Exists(fullPath) 
    //                    select context.LoadFromAssemblyPath(fullPath))
    //               .FirstOrDefault();
    //        };
    //    }

    //    public static void AddAssembly(string name)
    //    {
    //        lock (_lock) 
    //            _assemblyList = _assemblyList.Add(name);
    //    }

    //    public static void AddProbePath(string path)
    //    {
    //        lock (_lock)
    //            _probePaths = _probePaths.Add(path);
    //    }
    //}
}