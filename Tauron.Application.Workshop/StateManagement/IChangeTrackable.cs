namespace Tauron.Application.Workshop.StateManagement
{
    public interface IChangeTrackable
    {
        bool IsChanged { get; }
    }
}