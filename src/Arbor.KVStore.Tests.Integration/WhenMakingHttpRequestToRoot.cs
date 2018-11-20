using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVStore.Web;
using Xunit;

namespace Arbor.KVStore.Tests.Integration
{
    public class WhenMakingHttpRequestToRoot : IDisposable
    {
        public void Dispose()
        {
            _tempDir?.Refresh();

            if (_tempDir?.Exists == true)
            {
                _tempDir.Delete(true);
            }
        }

        private DirectoryInfo _tempDir;

        [Fact]
        public async Task ThenItShouldReturnHttpStatusCodeOk200()
        {
            _tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "kvstore" + Guid.NewGuid()));

            if (!_tempDir.Exists)
            {
                _tempDir.Create();
            }

            HttpStatusCode statusCode;
            using (var cts = new CancellationTokenSource())
            {
                int port = 5051;
                string httpPortArg = $"{ArgConstants.HttpPort}={port}";
                string dbDirArg = $"{ArgConstants.DbDir}={_tempDir.FullName}";

                string[] args = {
                    httpPortArg,
                    dbDirArg
                };

                using (App app = await AppStarter.CreateAndStartAsync(args, cts))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:{port}/"))
                    {
                        using (HttpResponseMessage httpResponseMessage =
                            await new HttpClient().SendAsync(request, cts.Token))
                        {
                            statusCode = httpResponseMessage.StatusCode;
                        }
                    }

                    cts.Cancel();

                    await app.DisposeAsync();
                }
            }

            Assert.Equal(HttpStatusCode.OK, statusCode);
        }
    }
}