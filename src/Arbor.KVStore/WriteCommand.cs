using System;
using System.Threading.Tasks;

namespace Arbor.KVStore
{
    internal class WriteCommand : ICustomCommand
    {
        private readonly App _app;

        public WriteCommand(App app)
        {
            _app = app;
        }

        public async Task ExecuteAsync()
        {
            Console.WriteLine("Enter client id");
            string clientId = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                Console.WriteLine("Enter key");
                string key = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                Console.WriteLine("Enter value");
                string value = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                await _app.PutAsync(new StoredValue(clientId, key, value));
            }
        }
    }
}