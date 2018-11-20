using System;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    public struct StoredValue : IStoredValue
    {
        public override string ToString()
        {
            return $"{Key}={Value}, client id {ClientId}";
        }

        public string ClientId { get; }

        public string Key { get; }

        public string Value { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ClientId)
                               && !string.IsNullOrWhiteSpace(Key);

        public StoredValue(
            [NotNull] string clientId,
            [NotNull] string key,
            [NotNull] string value)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(key));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
            }

            ClientId = clientId.Trim().ToLowerInvariant();
            Key = key.Trim();
            Value = value.Trim();
        }
    }
}