using System;
using System.Collections.Generic;
using Akka.Actor;
using Tauron.Akka;

namespace Tauron.Application.ServiceManager.Core.Model
{
    public sealed class CommandExutor : ExposedReceiveActor
    {
        private readonly Dictionary<Guid, ICommandTask> _tasks = new Dictionary<Guid, ICommandTask>();

        public CommandExutor()
        {
            Receive<ICommandTask>(ct =>
            {
                var id = Guid.NewGuid();
                _tasks[id] = ct;

                ct.Run().PipeTo(Self, 
                    failure: e => new TaskFailed(id, e), 
                    success:b => b ? (object) new TaskCompled(id) : new TaskFailed(id, null));
            });

            Receive<TaskFailed>(tf =>
            {
                if(_tasks.Remove(tf.Id, out var ct))
                    ct.ReportError(tf.Error);
            });

            Receive<TaskCompled>(tc =>
            {
                if (_tasks.Remove(tc.Id, out var ct))
                    ct.Finish();
            });
        }

        private sealed class TaskCompled
        {
            public Guid Id { get; }

            public TaskCompled(Guid id) => Id = id;
        }

        private sealed class TaskFailed
        {
            public Guid Id { get; }

            public Exception? Error { get; }

            public TaskFailed(Guid id, Exception? error)
            {
                Id = id;
                Error = error;
            }
        }
    }
}