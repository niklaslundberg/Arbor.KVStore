namespace Arbor.KVStore.Web
{
    public class StoredKeyInput
    {
        public StoredKeyInput(string clientId, string key)
        {
            ClientId = clientId;
            Key = key;
        }

        public string ClientId { get; }

        public string Key { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ClientId)
                               && !string.IsNullOrWhiteSpace(Key);

        public override string ToString()
        {
            return $"{nameof(ClientId)}: {ClientId}, {nameof(Key)}: {Key}, {nameof(IsValid)}: {IsValid}";
        }
    }
}