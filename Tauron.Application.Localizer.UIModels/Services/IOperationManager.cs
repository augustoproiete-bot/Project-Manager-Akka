using System.Collections.Generic;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IOperationManager
    {
        IEnumerable<RunningOperation> RunningOperations { get; }

        OperationController StartOperation(string name);

        OperationController? Find(string id);

        bool ShouldClear();

        void Clear();


        bool ShouldCompledClear();

        void CompledClear();
    }
}