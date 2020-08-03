namespace Tauron.Application.AkkNode.Services.Core
{
    public interface IInternalSerializable
    {
        void Write(ActorBinaryWriter writer);
    }
}