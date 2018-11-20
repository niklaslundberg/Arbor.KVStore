using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace Arbor.KVStore.Web
{
    public class ImportData
    {
        public string Comment { get; }

        public ImportData(string comment)
        {
            Comment = comment;
        }
    }
}