using Tauron.Application.Localizer.UIModels.lang;

namespace Tauron.Application.Localizer.UIModels.Services
{
    public sealed class OperationController
    {
        private readonly RunningOperation _operation;
        private readonly LocLocalizer _localizer;

        public string Id => _operation.Key;

        public OperationController(RunningOperation operation, LocLocalizer localizer)
        {
            _operation = operation;
            _localizer = localizer;
        }

        public void Compled(string? msg = null)
        {
            if (string.IsNullOrWhiteSpace(msg))
                msg = _localizer.OperationControllerSuccess;

            _operation.Operation = OperationStatus.Success;
            _operation.Status = msg;
        }

        public void Failed(string? msg = null)
        {
            if (string.IsNullOrWhiteSpace(msg))
                msg = _localizer.OperationControllerFailed;

            _operation.Operation = OperationStatus.Failed;
            _operation.Status = msg;
        }

        public void UpdateStatus(string msg)
        {
            if(_operation.Operation == OperationStatus.Running)
                _operation.Status = msg;
        }
    }
}