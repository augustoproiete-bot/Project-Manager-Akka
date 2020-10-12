using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ServiceManager.ProjectDeployment.Build
{
    public sealed class ProjectFinder
    {
        private readonly DirectoryInfo? _root;

        public ProjectFinder(DirectoryInfo root) => _root = root;

        public FileInfo? Search(string name)
        {
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
    }
}