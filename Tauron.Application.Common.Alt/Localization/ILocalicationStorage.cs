using System.Threading.Tasks;

namespace Tauron.Localization
{
    public interface ILocalicationStorage
    {
        Task Load(string source);

        string? Get(string key);
    }
}