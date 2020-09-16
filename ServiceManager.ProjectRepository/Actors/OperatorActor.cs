using System;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ServiceManager.ProjectRepository.Data;
using Tauron.Application.AkkNode.Services;
using Tauron.Application.Master.Commands.Repository;

namespace ServiceManager.ProjectRepository.Actors
{
    public sealed class OperatorActor : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private readonly IMongoCollection<RepositoryEntry> _repos;
        private readonly GridFSBucket _bucket;

        public OperatorActor(IMongoCollection<RepositoryEntry> repos, GridFSBucket bucket)
        {
            _repos = repos;
            _bucket = bucket;

            Receive<RegisterRepository>(r => TryExecute(r, "RegisterRepository", RegisterRepository));
        }

        private void RegisterRepository(RegisterRepository repository, Reporter reporter)
        {
            var data = _repos.AsQueryable().FirstOrDefault(e => e.RepoName == repository.RepoName);
        }

        private void TryExecute<TMessage>(TMessage msg, string name, Action<TMessage, Reporter> process) 
            where TMessage : RepositoryAction
        {
            var reporter = Reporter.CreateReporter(Context);
            reporter.Listen(msg.Listner);

            try
            {
                process(msg, reporter);
            }
            catch (Exception e)
            {
                _log.Error(e, "Repository Operation {Name} Failed {Repository}", name, msg.RepoName);
                reporter.Compled(OperationResult.Failure(e.Message));
            }
        }
    }
}