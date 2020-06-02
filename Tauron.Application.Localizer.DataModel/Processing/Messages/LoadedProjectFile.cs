using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class LoadedProjectFile : Operation
    {
        public LoadedProjectFile(string operationId, ProjectFile projectFile, Exception? errorReason, bool ok) : base(operationId)
        {
            ProjectFile = projectFile;
            ErrorReason = errorReason;
            Ok = ok;
        }

        public ProjectFile ProjectFile { get; }

        public Exception? ErrorReason { get; }

        public bool Ok { get; }
    }
}