using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class PreparedBuild
    {
        public BuildInfo BuildInfo { get; }

        public Project TargetProject { get; }

        public ProjectFile ProjectFile { get; }

        public string Operation { get; }

        public string TargetPath { get; }

        public PreparedBuild(BuildInfo buildInfo, Project targetProject, ProjectFile projectFile, string operation, string targetPath)
        {
            BuildInfo = buildInfo;
            TargetProject = targetProject;
            ProjectFile = projectFile;
            Operation = operation;
            TargetPath = new Uri(new Uri(projectFile.Source), targetPath).LocalPath;
        }
    }
}