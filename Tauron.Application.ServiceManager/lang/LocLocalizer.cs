using System.CodeDom.Compiler;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization;

namespace Tauron.Application.Localizer.Generated
{
	[PublicAPI, GeneratedCode("Localizer", "1")]
	public sealed partial class LocLocalizer
	{
		public sealed class MainWindowRes
		{
			private readonly Task<string> __LabelTitle;
			private readonly Task<string> __DockHeaderNodeView;
			private readonly Task<string> __DockHeaderSeedNodeView;
			private readonly Task<string> __LabelOnlineStatus;
			private readonly Task<string> __DockHeaderLogView;
			private readonly Task<string> __DockHeaderHostView;
			private readonly Task<string> __DockHeaderApplicationsView;
			private readonly Task<string> __DockHeaderConfigurationView;
			private readonly Task<string> __DockHeaderSetupBuilderView;
			public MainWindowRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_Title"));
				__DockHeaderNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_NodeView"));
				__DockHeaderSeedNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_SeedNodeView"));
				__LabelOnlineStatus = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_OnlineStatus"));
				__DockHeaderLogView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_LogView"));
				__DockHeaderHostView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_HostView"));
				__DockHeaderApplicationsView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_ApplicationsView"));
				__DockHeaderConfigurationView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_ConfigurationView"));
				__DockHeaderSetupBuilderView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_SetupBuilderView"));
			}
			public string LabelTitle => __LabelTitle.Result;
			public string DockHeaderNodeView => __DockHeaderNodeView.Result;
			public string DockHeaderSeedNodeView => __DockHeaderSeedNodeView.Result;
			public string LabelOnlineStatus => __LabelOnlineStatus.Result;
			public string DockHeaderLogView => __DockHeaderLogView.Result;
			public string DockHeaderHostView => __DockHeaderHostView.Result;
			public string DockHeaderApplicationsView => __DockHeaderApplicationsView.Result;
			public string DockHeaderConfigurationView => __DockHeaderConfigurationView.Result;
			public string DockHeaderSetupBuilderView => __DockHeaderSetupBuilderView.Result;
		}
		public sealed class MemberStatusRes
		{
			private readonly Task<string> __Joining;
			private readonly Task<string> __Up;
			private readonly Task<string> __Leaving;
			private readonly Task<string> __Exiting;
			private readonly Task<string> __Down;
			private readonly Task<string> __WeaklyUp;
			private readonly Task<string> __SelfOnline;
			private readonly Task<string> __SelfOffline;
			public MemberStatusRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Joining = LocLocalizer.ToString(loc.RequestTask("MemberStatus_Joining"));
				__Up = LocLocalizer.ToString(loc.RequestTask("MemberStatus_Up"));
				__Leaving = LocLocalizer.ToString(loc.RequestTask("MemberStatus_Leaving"));
				__Exiting = LocLocalizer.ToString(loc.RequestTask("MemberStatus_Exiting"));
				__Down = LocLocalizer.ToString(loc.RequestTask("MemberStatus_Down"));
				__WeaklyUp = LocLocalizer.ToString(loc.RequestTask("MemberStatus_WeaklyUp"));
				__SelfOnline = LocLocalizer.ToString(loc.RequestTask("MemberStatus_SelfOnline"));
				__SelfOffline = LocLocalizer.ToString(loc.RequestTask("MemberStatus_SelfOffline"));
			}
			public string Joining => __Joining.Result;
			public string Up => __Up.Result;
			public string Leaving => __Leaving.Result;
			public string Exiting => __Exiting.Result;
			public string Down => __Down.Result;
			public string WeaklyUp => __WeaklyUp.Result;
			public string SelfOnline => __SelfOnline.Result;
			public string SelfOffline => __SelfOffline.Result;
		}
		public sealed class NodeViewRes
		{
			private readonly Task<string> __LabelStatus;
			public NodeViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelStatus = LocLocalizer.ToString(loc.RequestTask("NodeView_Label_Status"));
			}
			public string LabelStatus => __LabelStatus.Result;
		}
		public sealed class SeedNodeViewRes
		{
			private readonly Task<string> __LabelAdd;
			private readonly Task<string> __LabelRemove;
			private readonly Task<string> __LabelCreateShortCut;
			private readonly Task<string> __ErrorEmptyHostName;
			public SeedNodeViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelAdd = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_Add"));
				__LabelRemove = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_Remove"));
				__LabelCreateShortCut = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_CreateShortCut"));
				__ErrorEmptyHostName = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Error_EmptyHostName"));
			}
			public string LabelAdd => __LabelAdd.Result;
			public string LabelRemove => __LabelRemove.Result;
			public string LabelCreateShortCut => __LabelCreateShortCut.Result;
			public string ErrorEmptyHostName => __ErrorEmptyHostName.Result;
		}
		public sealed class AddSeedDialogRes
		{
			private readonly Task<string> __LabelTitle;
			public AddSeedDialogRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("AddSeedDialog_Label_Title"));
			}
			public string LabelTitle => __LabelTitle.Result;
		}
		public sealed class HostViewRes
		{
			private readonly Task<string> __LabelName;
			private readonly Task<string> __LabelHostPath;
			private readonly Task<string> __ButtonApplications;
			private readonly Task<string> __GroupBoxCommands;
			private readonly Task<string> __HostCommandStopAll;
			public HostViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelName = LocLocalizer.ToString(loc.RequestTask("HostView_Label_Name"));
				__LabelHostPath = LocLocalizer.ToString(loc.RequestTask("HostView_Label_HostPath"));
				__ButtonApplications = LocLocalizer.ToString(loc.RequestTask("HostView_Button_Applications"));
				__GroupBoxCommands = LocLocalizer.ToString(loc.RequestTask("HostView_GroupBox_Commands"));
				__HostCommandStopAll = LocLocalizer.ToString(loc.RequestTask("HostView_HostCommand_StopAll"));
			}
			public string LabelName => __LabelName.Result;
			public string LabelHostPath => __LabelHostPath.Result;
			public string ButtonApplications => __ButtonApplications.Result;
			public string GroupBoxCommands => __GroupBoxCommands.Result;
			public string HostCommandStopAll => __HostCommandStopAll.Result;
		}
		public sealed class HostCommandRes
		{
			private readonly Task<string> __Running;
			private readonly Task<string> __Finish;
			private readonly Task<string> __UnkowenError;
			private readonly Task<string> __DialogCommandTitle;
			private readonly Task<string> __DiaologCommandStopAll;
			private readonly Task<string> __DialogCommandStartAll;
			private readonly Task<string> __CommandNameStartAll;
			private readonly Task<string> __CommandNameStopAll;
			private readonly Task<string> __CommandNameStartApp;
			private readonly Task<string> __CommandNameStopApp;
			public HostCommandRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Running = LocLocalizer.ToString(loc.RequestTask("HostCommand_Running"));
				__Finish = LocLocalizer.ToString(loc.RequestTask("HostCommand_Finish"));
				__UnkowenError = LocLocalizer.ToString(loc.RequestTask("HostCommand_UnkowenError"));
				__DialogCommandTitle = LocLocalizer.ToString(loc.RequestTask("HostCommand_Dialog_CommandTitle"));
				__DiaologCommandStopAll = LocLocalizer.ToString(loc.RequestTask("HostCommand_DiaologCommand_StopAll"));
				__DialogCommandStartAll = LocLocalizer.ToString(loc.RequestTask("HostCommand_Dialog_CommandStartAll"));
				__CommandNameStartAll = LocLocalizer.ToString(loc.RequestTask("HostCommand_CommandName_StartAll"));
				__CommandNameStopAll = LocLocalizer.ToString(loc.RequestTask("HostCommand_CommandName_StopAll"));
				__CommandNameStartApp = LocLocalizer.ToString(loc.RequestTask("HostCommand_CommandName_StartApp"));
				__CommandNameStopApp = LocLocalizer.ToString(loc.RequestTask("HostCommand_CommandName_StopApp"));
			}
			public string Running => __Running.Result;
			public string Finish => __Finish.Result;
			public string UnkowenError => __UnkowenError.Result;
			public string DialogCommandTitle => __DialogCommandTitle.Result;
			public string DiaologCommandStopAll => __DiaologCommandStopAll.Result;
			public string DialogCommandStartAll => __DialogCommandStartAll.Result;
			public string CommandNameStartAll => __CommandNameStartAll.Result;
			public string CommandNameStopAll => __CommandNameStopAll.Result;
			public string CommandNameStartApp => __CommandNameStartApp.Result;
			public string CommandNameStopApp => __CommandNameStopApp.Result;
		}
		public sealed class SelectHostAppDialogRes
		{
			private readonly Task<string> __LabelTitle;
			public SelectHostAppDialogRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("SelectHostAppDialog_Label_Title"));
			}
			public string LabelTitle => __LabelTitle.Result;
		}
		public sealed class InitialDialogRes
		{
			private readonly Task<string> __Title;
			private readonly Task<string> __MainTextOne;
			private readonly Task<string> __MainTextTwo;
			public InitialDialogRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Title = LocLocalizer.ToString(loc.RequestTask("InitialDialog_Title"));
				__MainTextOne = LocLocalizer.ToString(loc.RequestTask("InitialDialog_MainText_One"));
				__MainTextTwo = LocLocalizer.ToString(loc.RequestTask("InitialDialog_MainText_Two"));
			}
			public string Title => __Title.Result;
			public string MainTextOne => __MainTextOne.Result;
			public string MainTextTwo => __MainTextTwo.Result;
		}
		public sealed class ConfigurationViewRes
		{
			private readonly Task<string> __SetupGroupBoxHeader;
			private readonly Task<string> __SetupGroupBoxInfoText;
			private readonly Task<string> __TextboxDatabaseInfo;
			private readonly Task<string> __TextBlockConnectionString;
			private readonly Task<string> __ValidateMongoUrlButton;
			public ConfigurationViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__SetupGroupBoxHeader = LocLocalizer.ToString(loc.RequestTask("ConfigurationView_SetupGroupBox_Header"));
				__SetupGroupBoxInfoText = LocLocalizer.ToString(loc.RequestTask("ConfigurationView_SetupGroupBox_InfoText"));
				__TextboxDatabaseInfo = LocLocalizer.ToString(loc.RequestTask("ConfigurationView_Textbox_DatabaseInfo"));
				__TextBlockConnectionString = LocLocalizer.ToString(loc.RequestTask("ConfigurationView_TextBlock_ConnectionString"));
				__ValidateMongoUrlButton = LocLocalizer.ToString(loc.RequestTask("ConfigurationView_ValidateMongoUrl_Button"));
			}
			public string SetupGroupBoxHeader => __SetupGroupBoxHeader.Result;
			public string SetupGroupBoxInfoText => __SetupGroupBoxInfoText.Result;
			public string TextboxDatabaseInfo => __TextboxDatabaseInfo.Result;
			public string TextBlockConnectionString => __TextBlockConnectionString.Result;
			public string ValidateMongoUrlButton => __ValidateMongoUrlButton.Result;
		}
		public sealed class SetupBuilderViewRes
		{
			private readonly Task<string> __Title;
			private readonly Task<string> __HostName;
			private readonly Task<string> __AddSeed;
			private readonly Task<string> __Create;
			private readonly Task<string> __Applications;
			private readonly Task<string> __ErrorDuplicateHostName;
			private readonly Task<string> __ErrorEmptyHostName;
			private readonly Task<string> __AddShortcut;
			public SetupBuilderViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Title = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_Title"));
				__HostName = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_HostName"));
				__AddSeed = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_AddSeed"));
				__Create = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_Create"));
				__Applications = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_Applications"));
				__ErrorDuplicateHostName = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_Error_DuplicateHostName"));
				__ErrorEmptyHostName = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_Error_EmptyHostName"));
				__AddShortcut = LocLocalizer.ToString(loc.RequestTask("SetupBuilderView_AddShortcut"));
			}
			public string Title => __Title.Result;
			public string HostName => __HostName.Result;
			public string AddSeed => __AddSeed.Result;
			public string Create => __Create.Result;
			public string Applications => __Applications.Result;
			public string ErrorDuplicateHostName => __ErrorDuplicateHostName.Result;
			public string ErrorEmptyHostName => __ErrorEmptyHostName.Result;
			public string AddShortcut => __AddShortcut.Result;
		}
		public sealed class SeedProcessorRes
		{
			private readonly Task<string> __AddSeedUrlInvalidFromat;
			public SeedProcessorRes(ActorSystem system)
			{
				var loc = system.Loc();
				__AddSeedUrlInvalidFromat = LocLocalizer.ToString(loc.RequestTask("SeedProcessor_AddSeedUrl_InvalidFromat"));
			}
			public string AddSeedUrlInvalidFromat => __AddSeedUrlInvalidFromat.Result;
		}
		public sealed class ApplicationsViewRes
		{
			private readonly Task<string> __GlobalManagerHeader;
			private readonly Task<string> __HostsApplicationHeader;
			public ApplicationsViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__GlobalManagerHeader = LocLocalizer.ToString(loc.RequestTask("ApplicationsView_GlobalManager_Header"));
				__HostsApplicationHeader = LocLocalizer.ToString(loc.RequestTask("ApplicationsView_HostsApplication_Header"));
			}
			public string GlobalManagerHeader => __GlobalManagerHeader.Result;
			public string HostsApplicationHeader => __HostsApplicationHeader.Result;
		}
		public sealed class CommonRes
		{
			private readonly Task<string> __Error;
			private readonly Task<string> __Warning;
			private readonly Task<string> __Unkowen;
			private readonly Task<string> __Cancel;
			private readonly Task<string> __Ok;
			private readonly Task<string> __Querying;
			private readonly Task<string> __Back;
			private readonly Task<string> __Next;
			private readonly Task<string> __Apply;
			public CommonRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Error = LocLocalizer.ToString(loc.RequestTask("Common_Error"));
				__Warning = LocLocalizer.ToString(loc.RequestTask("Common_Warning"));
				__Unkowen = LocLocalizer.ToString(loc.RequestTask("Common_Unkowen"));
				__Cancel = LocLocalizer.ToString(loc.RequestTask("Common_Cancel"));
				__Ok = LocLocalizer.ToString(loc.RequestTask("Common_Ok"));
				__Querying = LocLocalizer.ToString(loc.RequestTask("Common_Querying"));
				__Back = LocLocalizer.ToString(loc.RequestTask("Common_Back"));
				__Next = LocLocalizer.ToString(loc.RequestTask("Common_Next"));
				__Apply = LocLocalizer.ToString(loc.RequestTask("Common_Apply"));
			}
			public string Error => __Error.Result;
			public string Warning => __Warning.Result;
			public string Unkowen => __Unkowen.Result;
			public string Cancel => __Cancel.Result;
			public string Ok => __Ok.Result;
			public string Querying => __Querying.Result;
			public string Back => __Back.Result;
			public string Next => __Next.Result;
			public string Apply => __Apply.Result;
		}
		public sealed class RepositoryMessagesRes
		{
			private readonly Task<string> __GetRepo;
			private readonly Task<string> __UpdateRepository;
			private readonly Task<string> __DownloadRepository;
			private readonly Task<string> __GetRepositoryFromDatabase;
			private readonly Task<string> __ExtractRepository;
			private readonly Task<string> __CompressRepository;
			private readonly Task<string> __UploadRepositoryToDatabase;
			public RepositoryMessagesRes(ActorSystem system)
			{
				var loc = system.Loc();
				__GetRepo = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_GetRepo"));
				__UpdateRepository = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_UpdateRepository"));
				__DownloadRepository = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_DownloadRepository"));
				__GetRepositoryFromDatabase = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_GetRepositoryFromDatabase"));
				__ExtractRepository = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_ExtractRepository"));
				__CompressRepository = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_CompressRepository"));
				__UploadRepositoryToDatabase = LocLocalizer.ToString(loc.RequestTask("RepositoryMessages_UploadRepositoryToDatabase"));
			}
			public string GetRepo => __GetRepo.Result;
			public string UpdateRepository => __UpdateRepository.Result;
			public string DownloadRepository => __DownloadRepository.Result;
			public string GetRepositoryFromDatabase => __GetRepositoryFromDatabase.Result;
			public string ExtractRepository => __ExtractRepository.Result;
			public string CompressRepository => __CompressRepository.Result;
			public string UploadRepositoryToDatabase => __UploadRepositoryToDatabase.Result;
		}
		public sealed class RepoErrorCodesRes
		{
			private readonly Task<string> __DuplicateRepository;
			private readonly Task<string> __InvalidRepoName;
			private readonly Task<string> __DatabaseNoRepoFound;
			private readonly Task<string> __GithubNoRepoFound;
			public RepoErrorCodesRes(ActorSystem system)
			{
				var loc = system.Loc();
				__DuplicateRepository = LocLocalizer.ToString(loc.RequestTask("RepoErrorCodes_DuplicateRepository"));
				__InvalidRepoName = LocLocalizer.ToString(loc.RequestTask("RepoErrorCodes_InvalidRepoName"));
				__DatabaseNoRepoFound = LocLocalizer.ToString(loc.RequestTask("RepoErrorCodes_DatabaseNoRepoFound"));
				__GithubNoRepoFound = LocLocalizer.ToString(loc.RequestTask("RepoErrorCodes_GithubNoRepoFound"));
			}
			public string DuplicateRepository => __DuplicateRepository.Result;
			public string InvalidRepoName => __InvalidRepoName.Result;
			public string DatabaseNoRepoFound => __DatabaseNoRepoFound.Result;
			public string GithubNoRepoFound => __GithubNoRepoFound.Result;
		}
		public sealed class DeploymentMessagesRes
		{
			private readonly Task<string> __RegisterRepository;
			private readonly Task<string> __BuildStart;
			private readonly Task<string> __BuildKilling;
			private readonly Task<string> __BuildCompled;
			private readonly Task<string> __BuildExtractingRepository;
			private readonly Task<string> __BuildRunBuilding;
			private readonly Task<string> __BuildTryFindProject;
			public DeploymentMessagesRes(ActorSystem system)
			{
				var loc = system.Loc();
				__RegisterRepository = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_RegisterRepository"));
				__BuildStart = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildStart"));
				__BuildKilling = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildKilling"));
				__BuildCompled = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildCompled"));
				__BuildExtractingRepository = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildExtractingRepository"));
				__BuildRunBuilding = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildRunBuilding"));
				__BuildTryFindProject = LocLocalizer.ToString(loc.RequestTask("DeploymentMessages_BuildTryFindProject"));
			}
			public string RegisterRepository => __RegisterRepository.Result;
			public string BuildStart => __BuildStart.Result;
			public string BuildKilling => __BuildKilling.Result;
			public string BuildCompled => __BuildCompled.Result;
			public string BuildExtractingRepository => __BuildExtractingRepository.Result;
			public string BuildRunBuilding => __BuildRunBuilding.Result;
			public string BuildTryFindProject => __BuildTryFindProject.Result;
		}
		public sealed class BuildErrorCodesRes
		{
			private readonly Task<string> __GeneralQueryFailed;
			private readonly Task<string> __QueryAppNotFound;
			private readonly Task<string> __QueryFileNotFound;
			private readonly Task<string> __GerneralCommandError;
			private readonly Task<string> __CommandErrorRegisterRepository;
			private readonly Task<string> __CommandDuplicateApp;
			private readonly Task<string> __CommandAppNotFound;
			private readonly Task<string> __GernalBuildError;
			private readonly Task<string> __BuildDotnetNotFound;
			private readonly Task<string> __BuildDotNetFailed;
			private readonly Task<string> __BuildProjectNotFound;
			private readonly Task<string> __DatabaseError;
			public BuildErrorCodesRes(ActorSystem system)
			{
				var loc = system.Loc();
				__GeneralQueryFailed = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_GeneralQueryFailed"));
				__QueryAppNotFound = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_QueryAppNotFound"));
				__QueryFileNotFound = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_QueryFileNotFound"));
				__GerneralCommandError = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_GerneralCommandError"));
				__CommandErrorRegisterRepository = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_CommandErrorRegisterRepository"));
				__CommandDuplicateApp = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_CommandDuplicateApp"));
				__CommandAppNotFound = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_CommandAppNotFound"));
				__GernalBuildError = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_GernalBuildError"));
				__BuildDotnetNotFound = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_BuildDotnetNotFound"));
				__BuildDotNetFailed = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_BuildDotNetFailed"));
				__BuildProjectNotFound = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_BuildProjectNotFound"));
				__DatabaseError = LocLocalizer.ToString(loc.RequestTask("BuildErrorCodes_DatabaseError"));
			}
			public string GeneralQueryFailed => __GeneralQueryFailed.Result;
			public string QueryAppNotFound => __QueryAppNotFound.Result;
			public string QueryFileNotFound => __QueryFileNotFound.Result;
			public string GerneralCommandError => __GerneralCommandError.Result;
			public string CommandErrorRegisterRepository => __CommandErrorRegisterRepository.Result;
			public string CommandDuplicateApp => __CommandDuplicateApp.Result;
			public string CommandAppNotFound => __CommandAppNotFound.Result;
			public string GernalBuildError => __GernalBuildError.Result;
			public string BuildDotnetNotFound => __BuildDotnetNotFound.Result;
			public string BuildDotNetFailed => __BuildDotNetFailed.Result;
			public string BuildProjectNotFound => __BuildProjectNotFound.Result;
			public string DatabaseError => __DatabaseError.Result;
		}
		public LocLocalizer(ActorSystem system)
		{
			var loc = system.Loc();
			 MainWindow = new MainWindowRes(system);
			 MemberStatus = new MemberStatusRes(system);
			 NodeView = new NodeViewRes(system);
			 SeedNodeView = new SeedNodeViewRes(system);
			 AddSeedDialog = new AddSeedDialogRes(system);
			 HostView = new HostViewRes(system);
			 HostCommand = new HostCommandRes(system);
			 SelectHostAppDialog = new SelectHostAppDialogRes(system);
			 InitialDialog = new InitialDialogRes(system);
			 ConfigurationView = new ConfigurationViewRes(system);
			 SetupBuilderView = new SetupBuilderViewRes(system);
			 SeedProcessor = new SeedProcessorRes(system);
			 ApplicationsView = new ApplicationsViewRes(system);
			 Common = new CommonRes(system);
			 RepositoryMessages = new RepositoryMessagesRes(system);
			 RepoErrorCodes = new RepoErrorCodesRes(system);
			 DeploymentMessages = new DeploymentMessagesRes(system);
			 BuildErrorCodes = new BuildErrorCodesRes(system);
		}
		public MainWindowRes MainWindow { get; }
		public MemberStatusRes MemberStatus { get; }
		public NodeViewRes NodeView { get; }
		public SeedNodeViewRes SeedNodeView { get; }
		public AddSeedDialogRes AddSeedDialog { get; }
		public HostViewRes HostView { get; }
		public HostCommandRes HostCommand { get; }
		public SelectHostAppDialogRes SelectHostAppDialog { get; }
		public InitialDialogRes InitialDialog { get; }
		public ConfigurationViewRes ConfigurationView { get; }
		public SetupBuilderViewRes SetupBuilderView { get; }
		public SeedProcessorRes SeedProcessor { get; }
		public ApplicationsViewRes ApplicationsView { get; }
		public CommonRes Common { get; }
		public RepositoryMessagesRes RepositoryMessages { get; }
		public RepoErrorCodesRes RepoErrorCodes { get; }
		public DeploymentMessagesRes DeploymentMessages { get; }
		public BuildErrorCodesRes BuildErrorCodes { get; }
		private static Task<string> ToString(Task<object?> task)
			=> task.ContinueWith(t => t.Result as string ?? string.Empty);
	}
}
