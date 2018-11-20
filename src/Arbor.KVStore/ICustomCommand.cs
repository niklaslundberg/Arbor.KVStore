using System.Threading.Tasks;

namespace Arbor.KVStore
{
    public interface ICustomCommand
    {
        Task ExecuteAsync();
    }
}