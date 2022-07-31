using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Output;
using Structur.Program;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Structur.Server
{
    public class Client
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _hostName;
        private readonly ClientSwapInput _clientSwapInput;
        private readonly Uri _ioUri;
        private readonly Uri _versionUri;
        private readonly StructureIO _io;
        private string _connectionStatus = "Ready";
        private string _currentStructureOutput;
        private StructureProgram _localProgram;
        private readonly StructureIO _localProgramIO;
        private Thread _localProgramExecutionThread;

        public Client(StructureIO io, string hostName, ClientSwapInput clientSwapInput, StructureProgram localProgram, StructureIO localProgramIO)
        {
            _io = io;
            _hostName = hostName;
            _clientSwapInput = clientSwapInput;
            _ioUri = new Uri(hostName + $"/{Server.ApiName}{IOController.ControllerName}");
            _versionUri = new Uri(hostName + $"/{Server.ApiName}/version");
            _io.YStartPosition = 0;
            _localProgram = localProgram;
            _localProgramIO = localProgramIO;
            _localProgramExecutionThread = new Thread(localProgram.Run);
        }

        public async Task RunAsync()
        {
            if (!await CheckClientServerVersionsMatchAsync()) return;

            await GetAndUpdateScreenFromServerAsync();
            _localProgramExecutionThread.Start();
            while (true)
            {
                var key = ReadKeyAndSetStatusToWaiting();
                await PostKeyAndSetStatusToReady(key);
                _io.ProcessInBackgroundWhileWaitingForInput();

                if (_clientSwapInput.IsInputLoaded())
                {
                    if (_localProgramIO.KeyCount > 
                    _clientSwapInput.SwapClients();
                    break;
                }
            }
            _localProgramExecutionThread.Join();
        }

        private async Task<bool> CheckClientServerVersionsMatchAsync()
        {
            _io.Write("Checking versions...");
            var clientVersion = StructureProgram.Version;
            var serverVersion = await GetVersionAsync();
            if (VersionError(nameof(clientVersion.Major), clientVersion.Major, serverVersion.Major)) return false;
            if (VersionError(nameof(clientVersion.Minor), clientVersion.Minor, serverVersion.Minor)) return false;
            if (VersionError(nameof(clientVersion.Build), clientVersion.Build, serverVersion.Build)) return false;
            return true;
        }

        private async Task<bool> CheckClientServerHistoryAsync()
        {
            _io.Write("Comparing local history with remote...");
            var serverVersion = await GetVersionAsync();

            if (_localProgramIO.KeyCount == serverVersion.KeyCount)
            {
                if (_localProgramIO.KeyHash == serverVersion.KeyHash)
                {
                    _io.Write("Histories synchronized...");
                    return true;
                }
            }
            else if (_localProgramIO.KeyCount < serverVersion.KeyCount)
            {
                // Get next set of keys
                // Swap out input with fake input temporarily
            }
            else if (_localProgramIO.KeyCount > serverVersion.KeyCount)
            {
                return false;
            }
        }

        private bool VersionError(string versionType, int clientVersion, int serverVersion)
        {
            if (clientVersion == serverVersion) return false;
            _io.Write($"Error connecting to server: {versionType} version of the client is not the same as the server.");
            _io.Write($"Client {versionType} version = {clientVersion}");
            _io.Write($"Server {versionType} version = {serverVersion}");
            return true;
        }

        private async Task GetAndUpdateScreenFromServerAsync()
        {
            var response = await GetScreenAsync();
            _currentStructureOutput = response.ConsoleText;
            UpdateScreen();
        }

        private ProgramInputData ReadKeyAndSetStatusToWaiting()
        {
            var key = _io.ProgramInput.ReadInput();
            _connectionStatus = "Waiting";
            UpdateScreen();
            return key;
        }

        private async Task PostKeyAndSetStatusToReady(ProgramInputData key)
        {
            var response = await PostInputAsync(key);
            _currentStructureOutput = response.ConsoleText;
            UpdateScreen();
        }

        private void UpdateScreen()
        {
            _io.ClearBuffer();
            _io.Write($"Connection to {_hostName} - {_connectionStatus}      ");
            _io.Write(_currentStructureOutput);
            _io.ClearStaleOutput();
        }

        private async Task<VersionResponse> GetVersionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_versionUri);
                return await GetObjectFromHttpResponse<VersionResponse>(response);
            }
            catch (SocketException e)
            {
                _io.Write("Error connecting to server: " + e.Message);
                return new VersionResponse();
            }
            catch (HttpRequestException e)
            {
                _io.Write("Error connecting to server: " + e.Message);
                return new VersionResponse();
            }
        }

        private async Task<InputResponse> PostInputAsync(ProgramInputData inputData)
        {
            try
            {
                var serializedInput = JsonSerializer.Serialize(inputData);
                using var content = new StringContent(serializedInput, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_ioUri, content);
                return await GetObjectFromHttpResponse<InputResponse>(response);
            }
            catch (SocketException e)
            {
                return GetFailedInputResponseFromException(e);
            }
            catch (HttpRequestException e)
            {
                return GetFailedInputResponseFromException(e);
            }
        }

        private async Task<InputResponse> GetScreenAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_ioUri);
                return await GetObjectFromHttpResponse<InputResponse>(response);
            }
            catch (SocketException e)
            {
                return GetFailedInputResponseFromException(e);
            }
            catch (HttpRequestException e)
            {
                return GetFailedInputResponseFromException(e);
            }
        }

        private InputResponse GetFailedInputResponseFromException(Exception e)
        {
            _connectionStatus = "Failed";
            var response = new InputResponse();
            response.ConsoleText = e.Message;
            return response;
        }

        private async Task<T> GetObjectFromHttpResponse<T>(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            _connectionStatus = "Ready";
            return JsonSerializer.Deserialize<T>(responseBody);
        }

        /*
         * 
         * #1: Connect to server and figure out:
         *       Whether client and server versions are the same
         *       Who has the olest history
         *       Compare the last # the server knows vs the actual number
         *       Who has a valid history
         * If the history is invalid:
         *      Run in local mode.
         * If the server has a more up to date history and its valid.
         *      
         * 
         * 
         * 
         */
    }
}
