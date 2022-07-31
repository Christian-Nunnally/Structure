using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using Swan.Logging;
using System;
using System.Threading;

namespace Structure.Server
{
    class Server : IDisposable
    {
        public const string ApiName = "structure-api";
        private readonly WebServer _embedIOServer;

        private Thread ServerThread { get; set; }

        public Server(string url, IOController controller)
        {
            _embedIOServer = CreateWebServer(url, controller);
        }

        public void RunInNewThread()
        {
            if (ServerThread != null) throw new InvalidOperationException("Server already running.");
            ServerThread = new Thread(() => _embedIOServer.RunAsync());
            ServerThread.Start();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "This class manages the lifecycle of the server.")]
        private static WebServer CreateWebServer(string url, IOController controller)
        {
            Logger.UnregisterLogger<ConsoleLogger>();
            var server = new WebServer(o => o
                .WithUrlPrefix(url)
                .WithMode(HttpListenerMode.EmbedIO))
                .WithLocalSessionManager()
                .WithWebApi($"/{ApiName}", m => m.WithController(() => controller))
                .WithStaticFolder("/", "index.html", true, m => m.WithContentCaching(true))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            
            server.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
            return server;
        }

        public void Dispose()
        {
            _embedIOServer?.Dispose();
        }
    }
}
