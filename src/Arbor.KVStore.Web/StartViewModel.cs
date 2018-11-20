using System.Collections.Immutable;

namespace Arbor.KVStore.Web
{
    public class StartViewModel
    {
        public StartViewModel(ClientId clientId, TempMessage tempMessage, ImmutableArray<ClientId> clients)
        {
            ClientId = clientId;
            TempMessage = tempMessage;
            Clients = clients;
        }

        public ClientId ClientId { get; }

        public TempMessage TempMessage { get; }

        public ImmutableArray<ClientId> Clients { get; }
    }
}