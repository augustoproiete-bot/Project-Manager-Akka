using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization;

namespace Tauron.Application.Localizer.UIModels.lang
{
    [PublicAPI, CompilerGenerated]
    public sealed class LocLocalizer
    {
        private readonly Task<string> _centerViewNewProjectInvalidNameMessage;

        private readonly Task<string> _centerViewRemoveProjectDialogMessage;

        private readonly Task<string> _centerViewRemoveProjectDialogTitle;

        private readonly Task<string> _centerViewSaveProjectOperation;

        private readonly Task<string> _commonError;

        private readonly Task<string> _commonUnkowen;

        private readonly Task<string> _commonWarnig;

        private readonly Task<string> _mainWindowCloseWarning;

        private readonly Task<string> _mainWindowMainMenuFileSaveAs;

        private readonly Task<string> _mainWindowModelLoadProjectOperation;

        private readonly Task<string> _mainWindowModelLoadProjectSourceEmpty;

        private readonly Task<string> _mainWindowTitle;

        private readonly Task<string> _newProjectDialogViewErrorDuplicate;

        private readonly Task<string> _openFileDialogViewDialogFilter;

        private readonly Task<string> _openFileDialogViewDialogTitle;

        private readonly Task<string> _openFileDialogViewHeaderNewPrefix;

        private readonly Task<string> _operationControllerFailed;

        private readonly Task<string> _operationControllerRunning;

        private readonly Task<string> _operationControllerSucess;

        private readonly Task<string> _projectViewLanguageBoxFirstLabel;

        private readonly Task<string> _newEntryDialogViewDuplicateError;

        private readonly Task<string> _newEntryDialogViewCharError;

        private readonly Task<string> _mainWindowAnalyerRuleSource;

        private readonly Task<string> _mainWindowAnalyerRuleSourceName;

        private readonly Task<string> _mainWindowBuildProjectLabel;

        private readonly Task<string> _mainWindowBuildProjectFolderBrowserDescription;

        private readonly Task<string> _mainWindowodelBuildProjectOperation;

        private readonly Task<string> _mainWindowBuildProjectGatherData;

        private readonly Task<string> _mainWindowBuildprojectNoData;

        private readonly Task<string> _mainWindowBuildProjectGenerateLangFile;

        private readonly Task<string> _mainWindowBuildProjectGenerateCsFile;

        private readonly Task<string> _mainWindowBuildProjectAgentCompled;

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
            _projectViewLanguageBoxFirstLabel = ToString(loc.RequestTask("ProjectView_LanguageBox_FirstLabel"));
            _newEntryDialogViewDuplicateError = ToString(loc.RequestTask("NewEntryDialogView_Duplicate_Error"));
            _newEntryDialogViewCharError = ToString(loc.RequestTask("NewEntryDialogView_Char_Error"));
            _mainWindowAnalyerRuleSource = ToString(loc.RequestTask("MainWindow_Analyer_Rule_Source"));
            _mainWindowAnalyerRuleSourceName = ToString(loc.RequestTask("MainWindow_Analyer_Rule_Source_Name"));
            _mainWindowBuildProjectLabel = ToString(loc.RequestTask("MainWindow_BuildProject_Label"));
            _mainWindowBuildProjectFolderBrowserDescription = ToString(loc.RequestTask("MainWindow_BuildProject_FolderBrowser_Description"));
            _mainWindowodelBuildProjectOperation = ToString(loc.RequestTask("MainWindowodel_BuildProject_Operation"));
            _mainWindowBuildProjectGatherData = ToString(loc.RequestTask("MainWindow_BuildProject_GatherData"));
            _mainWindowBuildprojectNoData = ToString(loc.RequestTask("MainWindow_Buildproject_NoData"));
            _mainWindowBuildProjectGenerateLangFile = ToString(loc.RequestTask("MainWindow_BuildProject_GenerateLangFile"));
            _mainWindowBuildProjectGenerateCsFile = ToString(loc.RequestTask("MainWindow_BuildProject_GenerateCsFile"));
            _mainWindowBuildProjectAgentCompled = ToString(loc.RequestTask("MainWindow_BuildProject_AgentCompled"));
        }

        public string MainWindowBuildProjectAgentCompled => _mainWindowBuildProjectAgentCompled.Result;

        public string MainWindowBuildProjectGenerateCsFile => _mainWindowBuildProjectGenerateCsFile.Result;

        public string MainWindowBuildProjectGenerateLangFile => _mainWindowBuildProjectGenerateLangFile.Result;

        public string MainWindowBuildprojectNoData => _mainWindowBuildprojectNoData.Result;

        public string MainWindowBuildProjectGatherData => _mainWindowBuildProjectGatherData.Result;

        public string MainWindowodelBuildProjectOperation => _mainWindowodelBuildProjectOperation.Result;

        public string MainWindowBuildProjectFolderBrowserDescription => _mainWindowBuildProjectFolderBrowserDescription.Result;

        public string MainWindowBuildProjectLabel => _mainWindowBuildProjectLabel.Result;

        public string MainWindowAnalyerRuleSourceName => _mainWindowAnalyerRuleSourceName.Result;

        public string MainWindowAnalyerRuleSource => _mainWindowAnalyerRuleSource.Result;

        public string NewEntryDialogViewCharError => _newEntryDialogViewCharError.Result;

        public string NewEntryDialogViewDuplicateError => _newEntryDialogViewDuplicateError.Result;

        public string OperationControllerSuccess => _operationControllerSucess.Result;

        public string OperationControllerFailed => _operationControllerFailed.Result;

        public string OperationControllerRunning => _operationControllerRunning.Result;

        public string MainWindowModelLoadProjectOperation => _mainWindowModelLoadProjectOperation.Result;

        public string OpenFileDialogViewDialogFilter => _openFileDialogViewDialogFilter.Result;

        public string OpenFileDialogViewDialogTitle => _openFileDialogViewDialogTitle.Result;

        public string CommonError => _commonError.Result;

        public string MainWindowModelLoadProjectSourceEmpty => _mainWindowModelLoadProjectSourceEmpty.Result;

        public string OpenFileDialogViewHeaderNewPrefix => _openFileDialogViewHeaderNewPrefix.Result;

        public string MainWindowMainMenuFileSaveAs => _mainWindowMainMenuFileSaveAs.Result;

        public string CenterViewSaveProjectOperation => _centerViewSaveProjectOperation.Result;

        public string NewProjectDialogViewErrorDuplicate => _newProjectDialogViewErrorDuplicate.Result;

        public string CenterViewRemoveProjectDialogTitle => _centerViewRemoveProjectDialogTitle.Result;

        public string CenterViewRemoveProjectDialogMessage => _centerViewRemoveProjectDialogMessage.Result;

        public string CenterViewNewProjectInvalidNameMessage => _centerViewNewProjectInvalidNameMessage.Result;

        public string CommonUnkowen => _commonUnkowen.Result;

        public string MainWindowTitle => _mainWindowTitle.Result;

        public string CommonWarnig => _commonWarnig.Result;

        public string MainWindowCloseWarning => _mainWindowCloseWarning.Result;

        public string ProjectViewLanguageBoxFirstLabel => _projectViewLanguageBoxFirstLabel.Result;

        private static Task<string> ToString(Task<object?> task) 
            => task.ContinueWith(t => t.Result as string ?? string.Empty);
    }
}