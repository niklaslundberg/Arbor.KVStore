using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;

namespace Arbor.KVStore.Web
{
    public static class AppStarter
    {
        public const string HttpPort = "arbor:kvstore:http-port";

        public static async Task<App> CreateAndStartAsync(
            string[] args,
            [NotNull] CancellationTokenSource cancellationTokenSource)
        {
            if (cancellationTokenSource == null)
            {
                throw new ArgumentNullException(nameof(cancellationTokenSource));
            }

            Logger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            App app = await App.CreateAsync(args, logger, cancellationTokenSource);

            await app.StartAsync(args);

            IWebHost webHost = CreateWebHostBuilder(args, app).Build();

            await webHost.StartAsync();

            var applicationLifetime = webHost.Services.GetService<IApplicationLifetime>();

            cancellationTokenSource.Token.Register(() => applicationLifetime.StopApplication());
            cancellationTokenSource.Token.Register(() => webHost.Dispose());

            applicationLifetime.ApplicationStopped.Register(() => app.CancellationTokenSource.Cancel());

            return app;
        }

        public static async Task<int> StartAsync(
            string[] args,
            CancellationTokenSource cancellationTokenSource = default)
        {
            Logger logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

            using (App app = await App.CreateAsync(args, logger, cancellationTokenSource))
            {
                IWebHost webHost = null;
                try
                {
                    int startExitCode = await app.StartAsync(args);

                    if (Environment.UserInteractive && args.Any(arg =>
                            arg.Equals(InteractiveSession.InteractiveArgument, StringComparison.OrdinalIgnoreCase)))
                    {
                        using (var interactiveSession = new InteractiveSession(app))
                        {
                            await interactiveSession.RunAsync();
                        }

                        if (Debugger.IsAttached)
                        {
                            Debugger.Break();
                        }
                    }

                    if (startExitCode != 0)
                    {
                        return startExitCode;
                    }

                    webHost = CreateWebHostBuilder(args, app).Build();

                    await webHost.StartAsync();

                    var applicationLifetime = webHost.Services.GetService<IApplicationLifetime>();

                    applicationLifetime.ApplicationStopped.Register(() => app.CancellationTokenSource.Cancel());

                    webHost.WaitForShutdown();
                }
                finally
                {
                    webHost?.Dispose();

                    await app.DisposeAsync();
                }
            }

            return 0;
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, App app)
        {
            int? httpPort = null;

            if (int.TryParse(args.SingleOrDefault(arg => arg.StartsWith($"{HttpPort}="))?.Split('=')
                    .LastOrDefault(),
                out int port))
            {
                httpPort = port;
            }

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureServices(s => s.AddSingleton(app))
                .UseKestrel(kestrelOptions =>
                {
                    if (httpPort.HasValue)
                    {
                        kestrelOptions.Listen(IPAddress.Any, httpPort.Value);
                    }
                })
                .UseStartup<Startup>();
        }
    }
}
