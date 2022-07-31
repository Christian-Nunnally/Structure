using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using Swan.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Structur.Server
{
    class Server
    {
        public const string ApiName = "structure-api";
        private readonly string _url;
        private readonly IOController _controller;

        private Thread ServerThread { get; set; }

        public Server(string url, IOController controller)
        {
            _url = url;
            _controller = controller;
        }

        public void RunInNewThread()
        {
            if (ServerThread != null) throw new InvalidOperationException("Server already running.");
            ServerThread = new Thread(async () => await CreateAndRunServerAsync());
            ServerThread.Start();
        }

        private async Task CreateAndRunServerAsync()
        {
            using var server = new WebServer(SetupOptions);
            ConfigureWebserver(server);
            await server.RunAsync();
        }

        private void SetupOptions(WebServerOptions options)
        {
            options
                .WithUrlPrefix(_url)
                .WithMode(HttpListenerMode.EmbedIO);
        }

        private WebServer ConfigureWebserver(WebServer webserver)
        {
            webserver = webserver
                .WithLocalSessionManager()
                .WithWebApi($"/{ApiName}", m => m.WithController(() => _controller))
                .WithStaticFolder("/", "index.html", true, m => m.WithContentCaching(true))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            webserver.StateChanged += (s, e) => $"WebServer New State - {e.NewState}".Info();
            return webserver;
        }
    }
}
