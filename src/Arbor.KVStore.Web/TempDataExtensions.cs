using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace Arbor.KVStore.Web
{
    public static class TempDataExtensions
    {
        public static TempMessage GetMessage(this ITempDataDictionary tempData)
        {
            return tempData.Get<TempMessage>(TempMessage.Key);
        }

        public static void PutMessage(this ITempDataDictionary tempData, string message)
        {
            tempData.Put(TempMessage.Key, new TempMessage(message));
        }

        public static void Put<T>(this ITempDataDictionary tempData, string key, T value) where T : class
        {
            tempData[key] = JsonConvert.SerializeObject(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            tempData.TryGetValue(key, out object o);
            return o == null ? null : JsonConvert.DeserializeObject<T>((string)o);
        }
    }
}