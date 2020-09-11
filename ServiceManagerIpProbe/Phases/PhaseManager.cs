namespace ServiceManagerIpProbe.Phases
{
    public sealed class PhaseManager<TContext>
    {
        private readonly Phase<TContext>[] _phases;

        public int Pos { get; private set; }

        public bool Completed => Pos == _phases.Length;

        public PhaseManager(params Phase<TContext>[] phases) => _phases = phases;

        public void RunNext(TContext context)
        {
            if(context is IHasTimeout timeout && timeout.IsTimeedOut)
                return;

            var phase = _phases[Pos];
            Pos++;
            phase.Run(context, this);
        }
    }
}