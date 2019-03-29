using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Spreads.Buffers;
using Spreads.LMDB;

namespace Arbor.KVStore
{
    public sealed class App : IDisposable, IAsyncDisposable
    {
        private const string MetaDb = "$meta";
        private const string ClientIdPrefix = "clientid_";
        private readonly LMDBEnvironment _lmdbEnvironment;
        private readonly ILogger _logger;

        private App(ILogger logger, LMDBEnvironment lmdbEnvironment, CancellationTokenSource cts)
        {
            _logger = logger;
            _lmdbEnvironment = lmdbEnvironment;
            CancellationTokenSource = cts;
        }

        public CancellationTokenSource CancellationTokenSource { get; }

        private async Task<int> PutDbAsync(ClientId clientId)
        {
            int exitCode = 0;
            byte[] keyAsBytes = Encoding.UTF8.GetBytes(ClientIdPrefix + clientId.Id);
            byte[] valueAsBytes = Encoding.UTF8.GetBytes(clientId.Id);

            var keyBuffer = new DirectBuffer(keyAsBytes);
            var valueBuffer = new DirectBuffer(valueAsBytes);

            using (Database db =
                _lmdbEnvironment.OpenDatabase(MetaDb, new DatabaseConfig(DbFlags.Create)))
            {
                await db.Environment.WriteAsync(tx =>
                {
                    try
                    {
                        db.Put(tx, ref keyBuffer, ref valueBuffer);

                        tx.Commit();

                        _logger.Information("Successfully stored client with id {Id}", clientId.Id);
                    }
                    catch (Exception ex)
                    {
                        tx.Abort();
                        _logger.Error(ex, "Aborted transaction");
                        exitCode = 1;
                    }
                });
            }

            return exitCode;
        }

        public static async Task<App> CreateAsync(
            string[] args,
            ILogger logger,
            CancellationTokenSource cancellationTokenSource)
        {
            string dbDir = args.FirstOrDefault(arg => arg.StartsWith(ArgConstants.DbDir))?.Split('=').LastOrDefault();

            if (string.IsNullOrWhiteSpace(dbDir))
            {
                dbDir = Environment.GetEnvironmentVariable(ArgConstants.DbDir);

                if (string.IsNullOrWhiteSpace(dbDir))
                {
                    throw new InvalidOperationException($"DbDir is not specified in arg or environment variable {ArgConstants.DbDir}");
                }
            }

            DirectoryInfo environmentDirectory = new DirectoryInfo(dbDir).EnsureExists();

            LMDBEnvironment environment = LMDBEnvironment.Create(environmentDirectory.FullName,
                LMDBEnvironmentFlags.WriteMap | LMDBEnvironmentFlags.NoSync);

            environment.MapSize = 126 * 1024 * 1024;

            return new App(logger, environment, cancellationTokenSource ?? new CancellationTokenSource());
        }

        public ImmutableArray<ClientId> GetClients()
        {
            var clients = new List<ClientId>();

            using (Database db =
                _lmdbEnvironment.OpenDatabase(MetaDb, new DatabaseConfig(DbFlags.Create)))
            {
                using (ReadOnlyTransaction tx = _lmdbEnvironment.BeginReadOnlyTransaction())
                {
                    using (ReadOnlyCursor cursor = db.OpenReadOnlyCursor(tx))
                    {
                        DirectBuffer key = default;
                        DirectBuffer value = default;

                        while (cursor.TryGet(ref key, ref value, CursorGetOption.Next))
                        {
                            string storedValue = Encoding.UTF8.GetString(value);
                            string storedKey = Encoding.UTF8.GetString(key);

                            if (storedKey.StartsWith(ClientIdPrefix, StringComparison.Ordinal))
                            {
                                clients.Add(new ClientId(storedValue));
                            }
                        }
                    }
                }
            }

            return clients.ToImmutableArray();
        }

        public ImmutableArray<StoredValue> ReadAllValues(ClientId clientId)
        {
            var storedValues = new List<StoredValue>();

            using (Database db =
                _lmdbEnvironment.OpenDatabase(clientId.Id, new DatabaseConfig(DbFlags.Create)))
            {
                using (ReadOnlyTransaction tx = _lmdbEnvironment.BeginReadOnlyTransaction())
                {
                    using (ReadOnlyCursor cursor = db.OpenReadOnlyCursor(tx))
                    {
                        DirectBuffer key = default;
                        DirectBuffer value = default;

                        while (cursor.TryGet(ref key, ref value, CursorGetOption.Next))
                        {
                            string storedValue = Encoding.UTF8.GetString(value);
                            string storedKey = Encoding.UTF8.GetString(key);
                            storedValues.Add(new StoredValue(clientId.Id, storedKey, storedValue));
                        }
                    }
                }
            }

            return storedValues.ToImmutableArray();
        }

        public async Task<int> PutAsync(StoredValue storedValue)
        {
            await EnsureClientExists(new ClientId(storedValue.ClientId));
            int exitCode = 0;
            byte[] keyAsBytes = Encoding.UTF8.GetBytes(storedValue.Key);
            byte[] valueAsBytes = Encoding.UTF8.GetBytes(storedValue.Value);

            var keyBuffer = new DirectBuffer(keyAsBytes);
            var valueBuffer = new DirectBuffer(valueAsBytes);

            using (Database db =
                _lmdbEnvironment.OpenDatabase(storedValue.ClientId, new DatabaseConfig(DbFlags.Create)))
            {
                await db.Environment.WriteAsync(tx =>
                {
                    try
                    {
                        db.Put(tx, ref keyBuffer, ref valueBuffer);

                        tx.Commit();

                        _logger.Information("Successfully stored item with key {Key}", storedValue.Key);
                    }
                    catch (Exception ex)
                    {
                        tx.Abort();
                        _logger.Error(ex, "Aborted transaction");
                        exitCode = 1;
                    }
                });
            }

            return exitCode;
        }

        public async Task<int> StartAsync(string[] args)
        {
            _lmdbEnvironment.Open();

            return 0;
        }

        public StoredValue ReadValue(StoredKey storedKey)
        {
            if (storedKey == default)
            {
                throw new ArgumentNullException(nameof(storedKey));
            }

            using (Database db =
                _lmdbEnvironment.OpenDatabase(storedKey.ClientId, new DatabaseConfig(DbFlags.Create)))
            {
                using (ReadOnlyTransaction tx = _lmdbEnvironment.BeginReadOnlyTransaction())
                {
                    var keyBuffer = new DirectBuffer(Encoding.UTF8.GetBytes(storedKey.Key));
                    if (db.TryGet(tx, ref keyBuffer, out DirectBuffer valueBuffer))
                    {
                        string value = Encoding.UTF8.GetString(valueBuffer);

                        return new StoredValue(storedKey.ClientId, storedKey.Key, value);
                    }

                    return default;
                }
            }
        }

        public async Task<StoredValue> EnsureClientExists(ClientId clientId)
        {
            using (Database db =
                _lmdbEnvironment.OpenDatabase(MetaDb, new DatabaseConfig(DbFlags.Create)))
            {
                using (ReadOnlyTransaction tx = _lmdbEnvironment.BeginReadOnlyTransaction())
                {
                    var keyBuffer = new DirectBuffer(Encoding.UTF8.GetBytes(ClientIdPrefix + clientId.Id));
                    if (!db.TryGet(tx, ref keyBuffer, out DirectBuffer _))
                    {
                        await PutDbAsync(clientId);
                    }

                    return default;
                }
            }
        }

        public async Task DeleteAsync(StoredKey key)
        {
            byte[] keyAsBytes = Encoding.UTF8.GetBytes(key.Key);

            var keyBuffer = new DirectBuffer(keyAsBytes);

            using (Database db =
                _lmdbEnvironment.OpenDatabase(key.ClientId, new DatabaseConfig(DbFlags.Create)))
            {
                await db.Environment.WriteAsync(tx =>
                {
                    try
                    {
                        if (!db.TryGet(tx, ref keyBuffer, out DirectBuffer _))
                        {
                            tx.Abort();
                            return;
                        }

                        db.Delete(tx, ref keyBuffer);

                        tx.Commit();

                        _logger.Information("Successfully deleted item with key {Key}", key.Key);
                    }
                    catch (Exception ex)
                    {
                        tx.Abort();
                        _logger.Error(ex, "Aborted transaction");
                    }
                });
            }
        }

        public async Task DisposeAsync()
        {
            await _lmdbEnvironment.Close();
        }

        public void Dispose()
        {
            if (_logger is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
