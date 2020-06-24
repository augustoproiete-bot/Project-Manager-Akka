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
		public sealed class TestRes
		{
			private readonly Task<string> __N1;
			public TestRes(ActorSystem system)
			{
				var loc = system.Loc();
				__N1 = LocLocalizer.ToString(loc.RequestTask("Test_N1"));
			}
			public string N1 => __N1.Result;
		}
		public sealed class CommonRes
		{
			private readonly Task<string> __N1;
			private readonly Task<string> __N2;
			private readonly Task<string> __N3;
			private readonly Task<string> __N4;
			public CommonRes(ActorSystem system)
			{
				var loc = system.Loc();
				__N1 = LocLocalizer.ToString(loc.RequestTask("Common_N1"));
				__N2 = LocLocalizer.ToString(loc.RequestTask("Common_N2"));
				__N3 = LocLocalizer.ToString(loc.RequestTask("Common_N3"));
				__N4 = LocLocalizer.ToString(loc.RequestTask("Common_N4"));
			}
			public string N1 => __N1.Result;
			public string N2 => __N2.Result;
			public string N3 => __N3.Result;
			public string N4 => __N4.Result;
		}
		public LocLocalizer(ActorSystem system)
		{
			var loc = system.Loc();
			 Test = new TestRes(system);
			 Common = new CommonRes(system);
		}
		public TestRes Test { get; }
		public CommonRes Common { get; }
		private static Task<string> ToString(Task<object?> task)
			=> task.ContinueWith(t => t.Result as string ?? string.Empty);
	}
}
