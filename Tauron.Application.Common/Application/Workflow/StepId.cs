using System.Diagnostics;
using JetBrains.Annotations;

namespace Tauron.Application.Workflow
{
    [PublicAPI]
    public struct StepId
    {
        //public static readonly StepId Null = new StepId();

        public static readonly StepId Fail = new("Fail");
        public static readonly StepId None = new("None");
        public static readonly StepId Finish = new("Finish");
        public static readonly StepId Loop = new("Loop");
        public static readonly StepId LoopEnd = new("LoopEnd");
        public static readonly StepId LoopContinue = new("LoopContinue");
        public static readonly StepId Skip = new("Skip");
        public static readonly StepId Start = new("Start");
        public static readonly StepId Waiting = new("Waiting");

        [DebuggerStepThrough]
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public StepId([NotNull] string name) : this()
        {
            Argument.NotNull(name, nameof(name));
            Name = name;
        }

        [NotNull] public string Name { get; }

        [DebuggerStepThrough]
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (!(obj is StepId)) return false;

            return ((StepId) obj).Name == Name;
        }

        public static bool operator ==(StepId id1, StepId id2) 
            => id1.Name == id2.Name;

        public static bool operator !=(StepId id1, StepId id2) 
            => id1.Name != id2.Name;

        [DebuggerStepThrough]
        public override string ToString() 
            => Name;
    }
}