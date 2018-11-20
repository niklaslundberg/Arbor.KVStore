using System;

namespace Arbor.KVStore
{
    public struct StoredKey : IEquatable<StoredKey>
    {
        public bool Equals(StoredKey other)
        {
            return string.Equals(ClientId, other.ClientId, StringComparison.Ordinal) &&
                   string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is StoredKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ClientId != null ? ClientId.GetHashCode() : 0) * 397) ^ (Key != null ? Key.GetHashCode() : 0);
            }
        }

        public static bool operator ==(StoredKey left, StoredKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StoredKey left, StoredKey right)
        {
            return !left.Equals(right);
        }

        public override string ToString()
        {
            return $"{nameof(ClientId)}: {ClientId}, {nameof(Key)}: {Key}";
        }

        public string ClientId { get; }

        public string Key { get; }

        public StoredKey(string clientId, string key)
        {
            ClientId = clientId.Trim().ToLowerInvariant();
            Key = key.Trim();
        }
    }
}