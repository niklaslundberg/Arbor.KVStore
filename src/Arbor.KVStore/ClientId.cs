using System;
using JetBrains.Annotations;

namespace Arbor.KVStore
{
    public sealed class ClientId : IEquatable<ClientId>
    {
        public ClientId([NotNull] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
            }

            Id = id.Trim().ToLowerInvariant();
        }

        public string Id { get; }

        public static bool operator ==(ClientId left, ClientId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ClientId left, ClientId right)
        {
            return !Equals(left, right);
        }

        public bool Equals(ClientId other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is ClientId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Id != null ? Id.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Id;
        }
    }
}