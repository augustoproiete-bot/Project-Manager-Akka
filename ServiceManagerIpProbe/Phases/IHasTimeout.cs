namespace ServiceManagerIpProbe.Phases
{
    public interface IHasTimeout
    {
        bool IsTimeedOut { get; }
    }
}