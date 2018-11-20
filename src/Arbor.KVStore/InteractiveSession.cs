using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    public sealed class InteractiveSession : IDisposable
    {
        public const string InteractiveArgument = "--interactive";

        private readonly App _app;

        public InteractiveSession([NotNull] App app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        public void Dispose()
        {
        }

        public async Task RunAsync()
        {
            while (!_app.CancellationTokenSource.IsCancellationRequested)
            {
                PrintOptions();
                ICustomCommand command = ParseCommand();
                if (command != null)
                {
                    await command.ExecuteAsync();
                }
            }
        }

        private ICustomCommand ParseCommand()
        {
            string line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            if (int.TryParse(line, out int option))
            {
                if (option == 0)
                {
                    _app.CancellationTokenSource.Cancel();
                    return null;
                }

                if (option == 1)
                {
                    return new ReadCommand(_app);
                }

                if (option == 2)
                {
                    return new WriteCommand(_app);
                }

                if (option == 3)
                {
                    return new ReadItemCommand(_app);
                }
            }

            return null;
        }

        private void PrintOptions()
        {
            Console.WriteLine("Options");
            Console.WriteLine("0. Exit");
            Console.WriteLine("1. Read all values");
            Console.WriteLine("2. Store value");
            Console.WriteLine("3. Read value from key");
        }
    }
}