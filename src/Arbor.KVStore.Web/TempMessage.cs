namespace Arbor.KVStore.Web
{
    public class TempMessage
    {
        public string Message { get; }
        public const string Key = nameof(TempMessage);

        public TempMessage(string message)
        {
            Message = message;
        }
    }
}