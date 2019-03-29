using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arbor.KVConfiguration.Schema.Json;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Arbor.KVStore.Web
{
    public class StartController : Controller
    {
        private const string ClientIdCookieName = "clientid";

        private readonly App _app;

        public StartController([NotNull] App app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        [HttpGet]
        [Route(RouteConstants.StartRoute, Name = RouteConstants.StartRouteName)]
        public async Task<IActionResult> Index()
        {
            TempMessage tempMessage = TempData.GetMessage();

            ImmutableArray<ClientId> clients = _app.GetClients();

            ClientId clientId = GetClientId() ?? clients.FirstOrDefault();

            SetCookie(clientId);

            return View(new StartViewModel(clientId, tempMessage, clients));
        }

        private void SetCookie(ClientId clientId)
        {
            if (clientId != null)
            {
                Response.Cookies.Append(ClientIdCookieName, clientId.Id);
            }
        }

        [Route(RouteConstants.ExportRoute, Name = RouteConstants.ExportRouteName)]
        [HttpGet]
        public IActionResult Export()
        {
            ImmutableArray<KeyValue> valuePairs = _app
                .ReadAllValues(GetClientId())
                .Select(pair => new KeyValue(pair.Key, pair.Value, null))
                .OrderBy(pair => pair.Key)
                .ThenBy(pair => pair.Value)
                .ToImmutableArray();


            var json = JsonConfigurationSerializer.Serialize(new ConfigurationItems("1.0", valuePairs));

            return new ContentResult
            {
                Content = json,
                ContentType = "application/json",
                StatusCode = 200
            };
        }

        [Route(RouteConstants.ImportRoute, Name = RouteConstants.ImportRouteName)]
        [HttpPost]
        public async Task<IActionResult> Import([FromBody] ImportData importData, IFormFile importFile)
        {
            ClientId clientId = GetClientId();

            if (clientId is null)
            {
                return RedirectToIndex();
            }

            if (importFile is null)
            {
                return RedirectToIndex();
            }

            if (importFile.Length == 0)
            {
                return RedirectToIndex();
            }

            string tempFileName = Path.GetTempFileName();

            using (var fileStream = new FileStream(tempFileName, FileMode.OpenOrCreate))
            {
                await importFile.CopyToAsync(fileStream);

                await fileStream.FlushAsync();
            }

            int imported = 0;
            try
            {
                ConfigurationItems configurationItems =
                    new KVConfiguration.JsonConfiguration.JsonFileReader(tempFileName).GetConfigurationItems();

                foreach (KeyValue keyValue in configurationItems.Keys)
                {
                    var storedValue = new StoredValue(clientId.Id, keyValue.Key, keyValue.Value);
                    await _app.PutAsync(storedValue);

                    imported++;
                }
            }
            catch (Exception ex)
            {
                TempData.Put(TempMessage.Key, new TempMessage(ex.Message));

                return RedirectToIndex();
            }
            finally
            {
                if (System.IO.File.Exists(tempFileName))
                {
                    System.IO.File.Delete(tempFileName);
                }
            }

            TempData.PutMessage($"Successfully imported file with {imported} key-value pairs");

            return RedirectToIndex();
        }

        [HttpPost]
        [Route(RouteConstants.ClientRoute, Name = RouteConstants.ClientRouteName)]
        public IActionResult PostClient([FromBody] ClientId clientId)
        {
            if (clientId != null)
            {
                SetCookie(clientId);
            }

            return RedirectToIndex();
        }

        [HttpPost]
        [Route(RouteConstants.CreateClientRoute, Name = RouteConstants.CreateClientRouteName)]
        public async Task<IActionResult> PostCreateClient([FromBody] ClientId clientId)
        {
            if (clientId != null)
            {
                var clients = _app.GetClients();

                if (!clients.Contains(clientId))
                {
                    await _app.EnsureClientExists(clientId);
                }

                SetCookie(clientId);
            }

            return RedirectToIndex();
        }

        [HttpGet]
        [Route(RouteConstants.ClientValuesRoute, Name = RouteConstants.ClientValuesRouteName)]
        public async Task<ImmutableArray<KeyValueView>> Values([FromRoute] string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ImmutableArray<KeyValueView>.Empty;
            }

            ImmutableArray<KeyValueView> valuePairs = _app
                .ReadAllValues(new ClientId(clientId))
                .Select(pair => new KeyValueView(pair.Key, pair.Value))
                .ToImmutableArray();

            return valuePairs;
        }

        [HttpPost]
        [Route(RouteConstants.DeleteRoute, Name = RouteConstants.DeleteRouteName)]
        public async Task<IActionResult> Delete([FromBody] StoredKeyInput storedKey)
        {
            if (storedKey?.IsValid == true)
            {
                var key = new StoredKey(storedKey.ClientId, storedKey.Key);
                await _app.DeleteAsync(key);
                TempData.PutMessage($"Successfully deleted item {key}");
            }
            else
            {
                TempData.PutMessage($"Invalid item {storedKey}");
            }

            return RedirectToIndex();
        }

        [HttpPost]
        [Route("~/")]
        public async Task<IActionResult> Put([FromBody] StoredValueInput storedValue)
        {
            if (storedValue?.IsValid == true)
            {
                var value = new StoredValue(storedValue.ClientId, storedValue.Key, storedValue.Value);
                await _app.PutAsync(value);
                TempData.PutMessage($"Successfully saved item {storedValue}");
            }
            else
            {
                TempData.PutMessage($"Invalid item {storedValue}");
            }

            return RedirectToIndex();
        }

        private RedirectToActionResult RedirectToIndex()
        {
            return RedirectToAction(nameof(Index));
        }

        private ClientId GetClientId()
        {
            string clientIdCookieValue = Request.Cookies[ClientIdCookieName];

            if (string.IsNullOrWhiteSpace(clientIdCookieValue))
            {
                return null;
            }

            var id = new ClientId(clientIdCookieValue);

            return id;
        }
    }

    public class KeyValueView
    {
        public string Key { get; }
        public string Value { get; }

        public KeyValueView(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
