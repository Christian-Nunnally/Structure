using Structure.IO;
using Structure.IO.Input;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Structure.Server
{
    public class Client
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _hostName;
        private readonly Uri _apiUri;
        private readonly StructureIO _io;
        private string _connectionStatus = "Ready";
        private string _currentStructureOutput;

        public Client(StructureIO io, string hostName)
        {
            _io = io;
            _hostName = hostName;
            _apiUri = new Uri(hostName + $"/{Server.ApiName}/{Controller.ControllerName}");
            _io.YStartPosition = 0;
        }

        public async Task RunAsync()
        {
            await GetAndUpdateScreenFromServerAsync();
            while (true)
            {
                var key = ReadKeyAndSetStatusToWaiting();
                await PostKeyAndSetStatusToReady(key);
                _io.ProcessInBackgroundWhileWaitingForInput();
            }
        }

        private async Task GetAndUpdateScreenFromServerAsync()
        {
            var response = await GetScreenAsync();
            _currentStructureOutput = response.ConsoleText;
            UpdateScreen();
        }

        private ProgramInputData ReadKeyAndSetStatusToWaiting()
        {
            var key = _io.ProgramInput.ReadKey();
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

        private async Task<InputResponse> PostInputAsync(ProgramInputData inputData)
        {
            try
            {
                var serializedInput = JsonSerializer.Serialize(inputData);
                using var content = new StringContent(serializedInput, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(_apiUri, content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var inputResponse = JsonSerializer.Deserialize<InputResponse>(responseBody);
                inputResponse.ConsoleText = inputResponse.ConsoleText;
                _connectionStatus = "Ready";
                return inputResponse;
            }
            catch (SocketException e)
            {
                _connectionStatus = "Failed";
                var response = new InputResponse();
                response.ConsoleText = e.Message;
                return response;
            }
            catch (HttpRequestException e)
            {
                _connectionStatus = "Failed";
                var response = new InputResponse();
                response.ConsoleText = e.Message;
                return response;
            }
        }

        private async Task<InputResponse> GetScreenAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(_apiUri);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();
                var inputResponse = JsonSerializer.Deserialize<InputResponse>(responseBody);
                inputResponse.ConsoleText = inputResponse.ConsoleText;
                _connectionStatus = "Ready";
                return inputResponse;
            }
            catch (SocketException e)
            {
                _connectionStatus = "Failed";
                var response = new InputResponse();
                response.ConsoleText = e.Message;
                return response;
            }
            catch (HttpRequestException e)
            {
                _connectionStatus = "Failed";
                var response = new InputResponse();
                response.ConsoleText = e.Message;
                return response;
            }
        }
    }
}
