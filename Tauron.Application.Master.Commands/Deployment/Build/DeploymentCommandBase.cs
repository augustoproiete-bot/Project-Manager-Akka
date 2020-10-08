using System;
using System.IO;
using System.Threading.Tasks;
using Akka.Actor;
using JetBrains.Annotations;
using Tauron.Akka;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.AkkNode.Services.Core;

namespace Tauron.Application.Master.Commands.Deployment.Build
{
    [PublicAPI]
    public abstract class DeploymentCommandBase<TResult> : DeplaymentAction, IDeploymentCommand
    {
        protected DeploymentCommandBase([NotNull] string appName) 
            : base(appName, ActorRefs.Nobody)
        {
        }

        protected DeploymentCommandBase([NotNull] BinaryReader reader, [NotNull] ExtendedActorSystem system) : base(reader, system)
        {
        }

        protected sealed override void ReadInternal(BinaryReader reader, BinaryManifest manifest)
        {
            base.ReadInternal(reader, manifest);
        }

        public Task<TResult> Send(DeploymentApi api, Action<string> messages, TimeSpan timeout)
        {
            var task = new TaskCompletionSource<TResult>();

            var listner = Reporter.CreateListner(ExposedReceiveActor.ExposedContext, messages, result =>
            {
                if (result.Ok)
                {
                    if (result.Outcome is TResult outcome)
                        task.SetResult(outcome);
                    else
                        task.SetException(new InvalidCastException(result.Outcome?.GetType().Name ?? "null-source"));
                }
                else
                {
                    task.SetException(new QueryFailedException(result.Error ?? "Unkowen"));
                }
            }, timeout);
            Listner = listner;

            api.SendAction(this);

            return task.Task;
        }
    }
}