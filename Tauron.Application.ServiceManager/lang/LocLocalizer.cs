using System.CodeDom.Compiler;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Localization;

namespace Tauron.Application.Localizer.Generated
{
	[PublicAPI, GeneratedCode("Localizer", "1")]
	internal sealed class LocLocalizer
	{
		public sealed class MainWindowRes
		{
			private readonly Task<string> __LabelTitle;
			private readonly Task<string> __DockHeaderNodeView;
			public MainWindowRes(ActorSystem system)
			{
				var loc = system.Loc();
				__LabelTitle = LocLocalizer.ToString(loc.RequestTask("MainWindow_Label_Title"));
				__DockHeaderNodeView = LocLocalizer.ToString(loc.RequestTask("MainWindow_DockHeader_NodeView"));
			}
			public string LabelTitle => __LabelTitle.Result;
			public string DockHeaderNodeView => __DockHeaderNodeView.Result;
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
			 Common = new CommonRes(system);
		}
		public MainWindowRes MainWindow { get; }
		public CommonRes Common { get; }
		private static Task<string> ToString(Task<object?> task)
			=> task.ContinueWith(t => t.Result as string ?? string.Empty);
	}
}
