using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using Swan.Logging;
using System;
using System.Diagnostics;
using System.Threading;

namespace Structure.Server
{
    class Server : IDisposable
    {
        public const string ApiName = "structure-api";

        private WebServer EmbedIOServer { get; set; }

        private Thread ServerThread { get; set; }

        private readonly string _url;

        public Server(string url, Controller controller)
        {
            EmbedIOServer = CreateWebServer(url, controller);
            _url = url;
        }

        public void RunInNewThread()
        {
            if (ServerThread != null)
            {
                ServerThread.Abort();
                ServerThread = null;
            }
  
            ServerThread = new Thread(RunServer);
            ServerThread.Start();
        }

        public void RunBrowser()
        {
            using var browser = new Process()
            {
                StartInfo = new ProcessStartInfo(_url) { UseShellExecute = true }
            };
            browser.Start();
        }

        private void RunServer()
        {
            EmbedIOServer.RunAsync();
        }

        public void StopServer()
        {
            ServerThread.Abort();
            EmbedIOServer.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "This class manages the lifecycle of the server.")]
        private static WebServer CreateWebServer(string url, Controller controller)
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

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EmbedIOServer.Dispose();
                    ServerThread.Abort();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
