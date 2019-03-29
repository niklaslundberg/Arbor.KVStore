using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Arbor.KVStore.Web;
using Xunit;
using Xunit.Abstractions;

namespace Arbor.KVStore.Tests.Integration
{
    public class WhenMakingHttpRequestToRoot : IDisposable
    {
        private readonly ITestOutputHelper _output;

        public WhenMakingHttpRequestToRoot(ITestOutputHelper output)
        {
            _output = output;
        }

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
            _tempDir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), $"kvstore{Guid.NewGuid()}"));

            if (!_tempDir.Exists)
            {
                _tempDir.Create();
            }

            HttpStatusCode statusCode;
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
            {
                int port = 5051;
                string httpPortArg = $"{AppStarter.HttpPort}={port}";
                string dbDirArg = $"{ArgConstants.DbDir}={_tempDir.FullName}";

                string[] args =
                {
                    httpPortArg,
                    dbDirArg
                };

                using (App app = await AppStarter.CreateAndStartAsync(args, cts))
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, $"http://localhost:{port}/"))
                    {
                        using (var httpClient = new HttpClient())
                        {
                            using (HttpResponseMessage httpResponseMessage =
                                await httpClient.SendAsync(request, cts.Token))
                            {
                                statusCode = httpResponseMessage.StatusCode;
                                _output.WriteLine($"Status code {statusCode}");

                                string content = await httpResponseMessage.Content.ReadAsStringAsync();

                                _output.WriteLine(content);
                            }
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
