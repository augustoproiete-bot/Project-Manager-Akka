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
			public MainWindowRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_Title"));
				__DockHeaderNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_NodeView"));
				__DockHeaderSeedNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_SeedNodeView"));
				__LabelOnlineStatus = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_OnlineStatus"));
				__DockHeaderLogView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_LogView"));
				__DockHeaderHostView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_HostView"));
			}
			public string LabelTitle => __LabelTitle.Result;
			public string DockHeaderNodeView => __DockHeaderNodeView.Result;
			public string DockHeaderSeedNodeView => __DockHeaderSeedNodeView.Result;
			public string LabelOnlineStatus => __LabelOnlineStatus.Result;
			public string DockHeaderLogView => __DockHeaderLogView.Result;
			public string DockHeaderHostView => __DockHeaderHostView.Result;
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
			public SeedNodeViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelAdd = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_Add"));
				__LabelRemove = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_Remove"));
				__LabelCreateShortCut = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_CreateShortCut"));
			}
			public string LabelAdd => __LabelAdd.Result;
			public string LabelRemove => __LabelRemove.Result;
			public string LabelCreateShortCut => __LabelCreateShortCut.Result;
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
		public sealed class CommonRes
		{
			private readonly Task<string> __Error;
			private readonly Task<string> __Warning;
			private readonly Task<string> __Unkowen;
			private readonly Task<string> __Cancel;
			private readonly Task<string> __Ok;
			private readonly Task<string> __Querying;
			public CommonRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Error = LocLocalizer.ToString(loc.RequestTask("Common_Error"));
				__Warning = LocLocalizer.ToString(loc.RequestTask("Common_Warning"));
				__Unkowen = LocLocalizer.ToString(loc.RequestTask("Common_Unkowen"));
				__Cancel = LocLocalizer.ToString(loc.RequestTask("Common_Cancel"));
				__Ok = LocLocalizer.ToString(loc.RequestTask("Common_Ok"));
				__Querying = LocLocalizer.ToString(loc.RequestTask("Common_Querying"));
			}
			public string Error => __Error.Result;
			public string Warning => __Warning.Result;
			public string Unkowen => __Unkowen.Result;
			public string Cancel => __Cancel.Result;
			public string Ok => __Ok.Result;
			public string Querying => __Querying.Result;
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
			 Common = new CommonRes(system);
		}
		public MainWindowRes MainWindow { get; }
		public MemberStatusRes MemberStatus { get; }
		public NodeViewRes NodeView { get; }
		public SeedNodeViewRes SeedNodeView { get; }
		public AddSeedDialogRes AddSeedDialog { get; }
		public HostViewRes HostView { get; }
		public HostCommandRes HostCommand { get; }
		public SelectHostAppDialogRes SelectHostAppDialog { get; }
		public CommonRes Common { get; }
		private static Task<string> ToString(Task<object?> task)
			=> task.ContinueWith(t => t.Result as string ?? string.Empty);
	}
}
