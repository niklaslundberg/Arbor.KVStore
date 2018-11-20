namespace Arbor.KVStore.Web
{
    public class StoredValueInput: IStoredValue
    {
        public StoredValueInput(
            string clientId,
            string key,
            string value)
        {
            ClientId = clientId;
            Key = key;
            Value = value;
        }

        public string ClientId { get; }

        public string Key { get; }

        public string Value { get; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ClientId)
                               && !string.IsNullOrWhiteSpace(Key);

        public override string ToString()
        {
            return $"{Key}={Value}, client id {ClientId}";
        }
    }
}