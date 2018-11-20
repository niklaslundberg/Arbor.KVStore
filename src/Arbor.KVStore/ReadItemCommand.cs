using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    internal class ReadItemCommand : ICustomCommand
    {
        private readonly App _app;

        public ReadItemCommand([NotNull] App app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Enter client id");
            string clientId = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                Console.WriteLine("Enter key");
                string key = Console.ReadLine();
                StoredValue storedValue= _app.ReadValue(new StoredKey(clientId, key));

                if (storedValue.Key is null)
                {
                    Console.WriteLine($"Could not find key {key}");
                }

                Console.WriteLine(storedValue);
            }
        }
    }
}