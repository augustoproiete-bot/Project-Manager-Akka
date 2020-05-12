using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization;

namespace Tauron.Application.Localizer.UIModels.lang
{
    [PublicAPI]
    public sealed class LocLocalizer
    {
        public LocLocalizer(ActorSystem system)
        {
            var loc = system.Loc();

            _operationControllerSucess = ToString(loc.RequestTask("OperationController_Success"));
            _operationControllerFailed = ToString(loc.RequestTask("OperationController_Failed"));
            _operationControllerRunning = ToString(loc.RequestTask("OperationController_Running"));
            _mainWindowModelLoadProjectOperation = ToString(loc.RequestTask("MainWindowModel_LoadProject_Operation"));
            _openFileDialogViewDialogFilter = ToString(loc.RequestTask("OpenFileDialogView_Dialog_Filter"));
            _openFileDialogViewDialogTitle = ToString(loc.RequestTask("OpenFileDialogView_Dialog_Title"));
        }

        private Task<string> ToString(Task<object?> task) => task.ContinueWith(t => t.Result as string ?? string.Empty);

        private readonly Task<string> _operationControllerSucess;

        public string OperationControllerSuccess => _operationControllerSucess.Result;

        private readonly Task<string> _operationControllerFailed;

        public string OperationControllerFailed => _operationControllerFailed.Result;

        private readonly Task<string> _operationControllerRunning;

        public string OperationControllerRunning => _operationControllerRunning.Result;

        private readonly Task<string> _mainWindowModelLoadProjectOperation;

        public string MainWindowModelLoadProjectOperation => _mainWindowModelLoadProjectOperation.Result;

        private readonly Task<string> _openFileDialogViewDialogFilter;

        public string OpenFileDialogViewDialogFilter => _openFileDialogViewDialogFilter.Result;

        private readonly Task<string> _openFileDialogViewDialogTitle;

        public string OpenFileDialogViewDialogTitle => _openFileDialogViewDialogTitle.Result;
    }
}