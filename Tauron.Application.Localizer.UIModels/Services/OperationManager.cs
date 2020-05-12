using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Tauron.Application.Localizer.UIModels.lang;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public sealed class OperationManager : IOperationManager
    {
        private sealed class OperationList : ObservableCollection<RunningOperation>
        {
            public int RunningOperations => this.Sum(ro => ro.Operation == OperationStatus.Running ? 1 : 0);

            protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
            {
                base.OnCollectionChanged(e);
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(RunningOperations)));
            }
        }

        private readonly LocLocalizer _localizer;
        private readonly Dispatcher _dispatcher;
        private readonly ObservableCollection<RunningOperation> _operations = new OperationList();

        public IEnumerable<RunningOperation> RunningOperations => _operations;

        public OperationManager(LocLocalizer localizer, Dispatcher dispatcher)
        {
            _localizer = localizer;
            _dispatcher = dispatcher;
        }

        public OperationController StartOperation(string name)
        {
            return _dispatcher.Invoke(() =>
                                      {
                                          var op = new RunningOperation(Guid.NewGuid().ToString(), name) {Status = _localizer.OperationControllerRunning};

                                          _operations.Add(op);
                                          return new OperationController(op, _localizer);
                                      });
        }

        public OperationController? Find(string id)
        {
            return _dispatcher.Invoke(() =>
                                      {
                                          var op = _operations.FirstOrDefault(op => op.Key == id);
                                          return op == null ? null : new OperationController(op, _localizer);
                                      });
        }

        public bool ShouldClear() 
            => _operations.Any(op => op.Operation == OperationStatus.Success);

        public void Clear()
        {
            _dispatcher.Invoke(() =>
                               {
                                   foreach (var operation in _operations.Where(op => op.Operation == OperationStatus.Success).ToArray()) 
                                       _operations.Remove(operation);
                               });
        }

        public bool ShouldCompledClear() 
            => _operations.Any(op => op.Operation != OperationStatus.Running);

        public void CompledClear()
        {
            _dispatcher.Invoke(() =>
                               {
                                   foreach (var operation in _operations.Where(op => op.Operation != OperationStatus.Running).ToArray())
                                       _operations.Remove(operation);
                               });
        }
    }
}