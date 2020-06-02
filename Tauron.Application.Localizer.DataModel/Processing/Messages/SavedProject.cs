using System;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    public sealed class SavedProject : Operation
    {
        public SavedProject(string operationId, bool ok, Exception? exception) : base(operationId)
        {
            Ok = ok;
            Exception = exception;
        }

        public bool Ok { get; }

        public Exception? Exception { get; }
    }
}