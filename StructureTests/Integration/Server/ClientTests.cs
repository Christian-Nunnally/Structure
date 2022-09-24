using Microsoft.VisualStudio.TestTools.UnitTesting;
using Structur.Program;
using Structur.Program.Utilities;
using Structur.Server;
using StructureTests.Integration;
using System.Threading;

namespace StructureTests.Server
{
    [TestClass]
    public class ClientTests : IntegrationTestBase
    {
        [TestInitialize]
        public void ClientTestsInitialize()
        {
            Tester.Settings.EnableClient = true;
        }

        [TestMethod]
        public void ClientEnabled_NoHostName_InvalidConfigurationError()
        {
            Tester.Run();
            Tester.Contains(Client.EmptyHostNameErrorMessage);
        }

        [TestMethod]
        public void ClientEnabled_InvalidHostname_UriFormatError()
        {
            Tester.Settings.ServerHostname = "localhost";

            Tester.Run();
            Tester.Contains("[ERROR] Invalid URI: The format of the URI could not be determined.");
        }

        [TestMethod]
        public void ClientEnabled_ServerOffline_ErrorConnectingToServerError()
        {
            Tester.Settings.ServerHostname = "http://localhost:9696/";

            Tester.Run();
            Tester.Contains("Error connecting to server");
        }

        [TestMethod]
        public void ClientEnabled_ServerOnline_ClientConnectedToServer()
        {
            var hostname = "http://localhost:9696/";
            using var _ = RunServer(hostname);
            Tester.Settings.ServerHostname = hostname;
            
            Tester.Run();
            Tester.Contains(Client.GetConnectionStatus(hostname, Client.ConnectionStatus.Ready));
        }

       [TestMethod]
        public void ClientEnabled_ServerOnline_ShowsMenu()
        {
            var hostname = "http://localhost:9696/";
            using var _ = RunServer(hostname);
            Tester.Settings.ServerHostname = hostname;

            Tester.Run();
            Tester.Contains(StructureProgram.TitleString);
        }

        private static DisposableAction RunServer(string hostname)
        {
            var server = new StructureTester();
            server.Settings.EnableWebServer = true;
            server.Settings.Hostname = hostname;
            var disposable = server.RunWithoutStopping();
            // Wait for the server to start. Ideally this should not be required.
            Thread.Sleep(1000);
            return disposable;
        }
    }
}