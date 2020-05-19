﻿using System.Threading.Tasks;
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
            _commonError = ToString(loc.RequestTask("Common_Error"));
            _mainWindowModelLoadProjectSourceEmpty = ToString(loc.RequestTask("MainWindowModel_LoadProject_SourceEmpty"));
            _openFileDialogViewHeaderNewPrefix = ToString(loc.RequestTask("OpenFileDialogView_Header_NewPrefix"));
            _mainWindowMainMenuFileSaveAs = ToString(loc.RequestTask("MainWindow_MainMenu_File_SaveAs"));
            _centerViewSaveProjectOperation = ToString(loc.RequestTask("CenterView_SaveProject_Operation"));
            _newProjectDialogViewErrorDuplicate = ToString(loc.RequestTask("NewProjectDialogView_Error_Duplicate"));
            _centerViewRemoveProjectDialogTitle = ToString(loc.RequestTask("CenterView_RemoveProjectDialog_Title"));
            _centerViewRemoveProjectDialogMessage = ToString(loc.RequestTask("CenterView_RemoveProjectDialog_Message"));
            _centerViewNewProjectInvalidNameMessage = ToString(loc.RequestTask("CenterView_NewProject_InvalidName_Message"));
            _commonUnkowen = ToString(loc.RequestTask("Common_Unkowen"));
            _mainWindowTitle = ToString(loc.RequestTask("MainWindow_Title"));
            _commonWarnig = ToString(loc.RequestTask("Common_Warnig"));
            _mainWindowCloseWarning = ToString(loc.RequestTask("MainWindow_Close_Warning"));
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

        private readonly Task<string> _commonError;

        public string CommonError => _commonError.Result;

        private readonly Task<string> _mainWindowModelLoadProjectSourceEmpty;

        public string MainWindowModelLoadProjectSourceEmpty => _mainWindowModelLoadProjectSourceEmpty.Result;

        private readonly Task<string> _openFileDialogViewHeaderNewPrefix;

        public string OpenFileDialogViewHeaderNewPrefix => _openFileDialogViewHeaderNewPrefix.Result;

        private readonly Task<string> _mainWindowMainMenuFileSaveAs;

        public string MainWindowMainMenuFileSaveAs => _mainWindowMainMenuFileSaveAs.Result;

        private readonly Task<string> _centerViewSaveProjectOperation;

        public string CenterViewSaveProjectOperation => _centerViewSaveProjectOperation.Result;

        private readonly Task<string> _newProjectDialogViewErrorDuplicate;

        public string NewProjectDialogViewErrorDuplicate => _newProjectDialogViewErrorDuplicate.Result;

        private readonly Task<string> _centerViewRemoveProjectDialogTitle;

        public string CenterViewRemoveProjectDialogTitle => _centerViewRemoveProjectDialogTitle.Result;

        private readonly Task<string> _centerViewRemoveProjectDialogMessage;

        public string CenterViewRemoveProjectDialogMessage => _centerViewRemoveProjectDialogMessage.Result;

        private readonly Task<string> _centerViewNewProjectInvalidNameMessage;

        public string CenterViewNewProjectInvalidNameMessage => _centerViewNewProjectInvalidNameMessage.Result;

        private readonly Task<string> _commonUnkowen;

        public string CommonUnkowen => _commonUnkowen.Result;

        private readonly Task<string> _mainWindowTitle;

        public string MainWindowTitle => _mainWindowTitle.Result;

        private readonly Task<string> _commonWarnig;

        public string CommonWarnig => _commonWarnig.Result;

        private readonly Task<string> _mainWindowCloseWarning;

        public string MainWindowCloseWarning => _mainWindowCloseWarning.Result;
    }
}