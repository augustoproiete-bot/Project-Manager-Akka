using System;
using Akka.Actor;
using Akka.Util;
using JetBrains.Annotations;

namespace Tauron.Akka
{
    [PublicAPI]
    public abstract class BaseActorRef<TActor> : IActorRef
        where TActor : ActorBase
    {
    private readonly ActorRefFactory<TActor> _builder;
    private IActorRef _ref = ActorRefs.Nobody;
    private bool _isInit;

    protected BaseActorRef(ActorRefFactory<TActor> actorBuilder)
        => _builder = actorBuilder;

    protected virtual bool IsSync => false;

    public void Tell(object message, IActorRef sender) => _ref.Tell(message, sender);

    public bool Equals(IActorRef other) => _ref.Equals(other);

    public int CompareTo(IActorRef other) => _ref.CompareTo(other);

    public ISurrogate ToSurrogate(ActorSystem system) => _ref.ToSurrogate(system);

    public int CompareTo(object obj) => _ref.CompareTo(obj);

    public ActorPath Path => _ref.Path;

    public virtual void Init(string? name = null)
    {
        CheckIsInit();
        _ref = _builder.Create(IsSync, name);
        _isInit = true;
    }

    public virtual void Init(IActorRefFactory factory, string? name = null)
    {
        CheckIsInit();
        _ref = factory.ActorOf(_builder.CreateProps(IsSync), name);
        _isInit = true;
    }

    protected void CheckIsInit()
    {
        if (_isInit)
            throw new InvalidOperationException("ActorRef is Init");
    }
    }
}