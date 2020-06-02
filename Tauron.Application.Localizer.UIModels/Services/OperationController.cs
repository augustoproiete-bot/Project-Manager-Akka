using System;
using Tauron.Application.Localizer.UIModels.lang;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public sealed class OperationController
    {
        private readonly LocLocalizer _localizer;
        private readonly RunningOperation _operation;

        private Action? _statusChanged;

        public OperationController(RunningOperation operation, LocLocalizer localizer, Action statusChanged)
        {
            _operation = operation;
            _localizer = localizer;
            if (_operation.Operation == OperationStatus.Running)
                _statusChanged = statusChanged;
        }

        public string Id => _operation.Key;

        public void Compled(string? msg = null)
        {
            if (_operation.Operation != OperationStatus.Running)
                return;

            if (string.IsNullOrWhiteSpace(msg))
                msg = _localizer.OperationControllerSuccess;

            _operation.Operation = OperationStatus.Success;
            _operation.Status = msg;
            _statusChanged?.Invoke();
            _statusChanged = null;
        }

        public void Failed(string? msg = null)
        {
            if (_operation.Operation != OperationStatus.Running)
                return;

            if (string.IsNullOrWhiteSpace(msg))
                msg = _localizer.OperationControllerFailed;

            _operation.Operation = OperationStatus.Failed;
            _operation.Status = msg;
            _statusChanged?.Invoke();
            _statusChanged = null;
        }

        public void UpdateStatus(string msg)
        {
            if (_operation.Operation == OperationStatus.Running)
                _operation.Status = msg;
        }
    }
}