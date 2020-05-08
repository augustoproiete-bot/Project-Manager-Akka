using Akka.Actor;
using Autofac;
using JetBrains.Annotations;
using Tauron.Application.Wpf.Model;
using Tauron.Application.Wpf.UI;

namespace Tauron.Application.Wpf
{
    [PublicAPI]
    public static class AutofacExtensions
    {
        public static void RegisterView<TView, TModel>(this  ContainerBuilder builder)
            where TView : notnull where TModel : ActorBase
        {
            AutoViewLocation.AddPair(typeof(TView), typeof(TModel));

            builder.RegisterType<TView>().As<TView>();
            builder.RegisterType<ViewModelActorRef<TModel>>().As<IViewModel<TModel>>();
        }
    }
}