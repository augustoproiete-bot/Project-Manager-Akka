namespace Tauron.Application.Workshop.StateManagement
{
    public interface IStateEntity
    {
        bool IsDeleted { get; }

        string Id { get; }
    }
}