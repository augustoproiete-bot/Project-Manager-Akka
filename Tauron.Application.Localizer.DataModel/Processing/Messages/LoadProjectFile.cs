using JetBrains.Annotations;

namespace Tauron.Application.Localizer.DataModel.Processing
{
    [PublicAPI]
    public sealed class LoadProjectFile : Operation
    {
        public string Source { get; }

        public LoadProjectFile(string operationId, string source) : base(operationId) => Source = source;
    }
}