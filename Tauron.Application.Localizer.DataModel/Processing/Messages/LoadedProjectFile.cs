using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class LoadedProjectFile : Operation
    {
        public ProjectFile ProjectFile { get; }

        public Exception? ErrorReason { get; }

        public bool Ok { get; }

        public LoadedProjectFile(string operationId, ProjectFile projectFile, Exception? errorReason, bool ok) : base(operationId)
        {
            ProjectFile = projectFile;
            ErrorReason = errorReason;
            Ok = ok;
        }
    }
}