using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    internal class ReadCommand : ICustomCommand
    {
        private readonly App _app;

        public ReadCommand([NotNull] App app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Enter client id");
            string clientId = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                ImmutableArray<StoredValue> immutableArray = _app.ReadAllValues(new ClientId(clientId));

                if (immutableArray.Length == 0)
                {
                    Console.WriteLine("No items stored");
                }
                else
                {
                    foreach (StoredValue storedValue in immutableArray)
                    {
                        Console.WriteLine(storedValue);
                    }
                }
            }
        }
    }
}