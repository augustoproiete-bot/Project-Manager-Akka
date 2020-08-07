using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace SimpleHostSetup.Impl
{
    public sealed class ProjectFinder
    {
        private readonly ILogger _logger = Log.ForContext<ProjectFinder>();

        private readonly string _searchStart;
        private readonly string _rootFile;
        private DirectoryInfo? _root;

        public ProjectFinder(string? searchStart, string rootFile)
        {
            if (string.IsNullOrWhiteSpace(rootFile))
                throw new ArgumentNullException(nameof(rootFile), "No Root File Provided");

            if (string.IsNullOrWhiteSpace(searchStart))
                throw new ArgumentNullException(nameof(searchStart), "No Search Path Provided");

            _searchStart = searchStart;
            _rootFile = rootFile;
        }

        public FileInfo? Search(string name)
        {
            _logger.Information("Try Find Project {Name}", name);

            if (_root == null) 
                SeekRoot();

            var directorys = new Queue<DirectoryInfo>();

            var current = _root;

            while (current != null)
            {
                foreach (var file in current.EnumerateFiles("*.*"))
                {
                    if (file.Name.Contains(name))
                        return file;
                }

                foreach (var directory in current.EnumerateDirectories()) 
                    directorys.Enqueue(directory);


                if (directorys.Count == 0)
                    break;
                
                current = directorys.Dequeue();
            }

            return null;
        }

        private void SeekRoot()
        {
            _logger.Information("Try Find Root {RootName}", _rootFile);

            var dic = new DirectoryInfo(_searchStart);
            while (dic != null)
            {
                if (dic.Exists && dic.EnumerateFiles().Any(f => f.Name == _rootFile))
                {
                    _root = dic;
                    _logger.Information("Root Directory Found {Dic}", dic.FullName);
                    break;
                }

                dic = dic.Parent;
            }

            if (dic == null)
                throw new InvalidOperationException("Not Root Directory Found");
        }
    }
}