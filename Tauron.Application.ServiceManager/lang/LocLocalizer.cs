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
			public MainWindowRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_Title"));
				__DockHeaderNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_NodeView"));
				__DockHeaderSeedNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_SeedNodeView"));
			}
			public string LabelTitle => __LabelTitle.Result;
			public string DockHeaderNodeView => __DockHeaderNodeView.Result;
			public string DockHeaderSeedNodeView => __DockHeaderSeedNodeView.Result;
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
			public SeedNodeViewRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelAdd = LocLocalizer.ToString(loc.RequestTask("SeedNodeView_Label_Add"));
			}
			public string LabelAdd => __LabelAdd.Result;
		}
		public sealed class CommonRes
		{
			private readonly Task<string> __Error;
			private readonly Task<string> __Warning;
			private readonly Task<string> __Unkowen;
			private readonly Task<string> __Cancel;
			private readonly Task<string> __Ok;
			public CommonRes(ActorSystem system)
			{
				var loc = system.Loc();
				__Error = LocLocalizer.ToString(loc.RequestTask("Common_Error"));
				__Warning = LocLocalizer.ToString(loc.RequestTask("Common_Warning"));
				__Unkowen = LocLocalizer.ToString(loc.RequestTask("Common_Unkowen"));
				__Cancel = LocLocalizer.ToString(loc.RequestTask("Common_Cancel"));
				__Ok = LocLocalizer.ToString(loc.RequestTask("Common_Ok"));
			}
			public string Error => __Error.Result;
			public string Warning => __Warning.Result;
			public string Unkowen => __Unkowen.Result;
			public string Cancel => __Cancel.Result;
			public string Ok => __Ok.Result;
		}
		public LocLocalizer(ActorSystem system)
		{
			var loc = system.Loc();
			 MainWindow = new MainWindowRes(system);
			 MemberStatus = new MemberStatusRes(system);
			 NodeView = new NodeViewRes(system);
			 SeedNodeView = new SeedNodeViewRes(system);
			 Common = new CommonRes(system);
		}
		public MainWindowRes MainWindow { get; }
		public MemberStatusRes MemberStatus { get; }
		public NodeViewRes NodeView { get; }
		public SeedNodeViewRes SeedNodeView { get; }
		public CommonRes Common { get; }
		private static Task<string> ToString(Task<object?> task)
			=> task.ContinueWith(t => t.Result as string ?? string.Empty);
	}
}
