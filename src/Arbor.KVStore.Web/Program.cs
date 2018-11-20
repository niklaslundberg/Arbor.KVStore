using System.Threading.Tasks;

namespace Arbor.KVStore.Web
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            return AppStarter.StartAsync(args);
        }
    }
}