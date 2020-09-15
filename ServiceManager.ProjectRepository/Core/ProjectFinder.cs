using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServiceManager.ProjectRepository.Core
{
    public sealed class ProjectFinder
    {
        private string _searchStart = string.Empty;
        private string _rootFile = string.Empty;
        private Action<string, object[]> _logger = (s, objects) => { };
        private DirectoryInfo? _root;

        public void Init(string? searchStart, string rootFile, Action<string, object[]> logger)
        {
            if (string.IsNullOrWhiteSpace(rootFile))
                throw new ArgumentNullException(nameof(rootFile), "No Root File Provided");

            if (string.IsNullOrWhiteSpace(searchStart))
                throw new ArgumentNullException(nameof(searchStart), "No Search Path Provided");

            _searchStart = searchStart;
            _rootFile = rootFile;
            _logger = logger;
        }

        private void Log(string template, params object[] args) => _logger(template, args);

        public FileInfo? Search(string name)
        {
            Log("Try Find Project {Name}", name);

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
            Log("Try Find Root {RootName}", _rootFile);

            var dic = new DirectoryInfo(_searchStart);
            while (dic != null)
            {
                if (dic.Exists && dic.EnumerateFiles().Any(f => f.Name == _rootFile))
                {
                    _root = dic;
                    Log("Root Directory Found {Dic}", dic.FullName);
                    break;
                }

                dic = dic.Parent;
            }

            if (dic == null)
                throw new InvalidOperationException("Not Root Directory Found");
        }
    }
}