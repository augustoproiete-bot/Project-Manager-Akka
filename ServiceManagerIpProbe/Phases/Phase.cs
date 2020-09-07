namespace ServiceManagerIpProbe.Phases
{
    public abstract class Phase<TContext>
    {
        public abstract void Run(TContext context, PhaseManager<TContext> manager);
    }
}