using System;
using System.Collections.Generic;
using Tauron.Application.Wpf.Model;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public interface IOperationManager
    {
        IEnumerable<RunningOperation> RunningOperations { get; }

        OperationController StartOperation(string name);

        OperationController? Find(string id);

        CommandQuery ShouldClear(CommandQueryBuilder builder, Action<IDisposable> subscription);

        void Clear();


        CommandQuery ShouldCompledClear(CommandQueryBuilder builder, Action<IDisposable> subscription);

        void CompledClear();
    }
}