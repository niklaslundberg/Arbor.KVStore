using System.Threading.Tasks;

namespace Arbor.KVStore
{
    public interface IAsyncDisposable
    {
        Task DisposeAsync();
    }
}