namespace Arbor.KVStore
{
    public interface IStoredValue
    {
        string ClientId { get; }

        string Key { get; }

        string Value { get; }

        bool IsValid { get; }
    }
}