using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Structur.IO;
using Structur.IO.Input;
using Structur.IO.Persistence;
using Structur.Program;
using Structur.Program.Utilities;
using Structur.Server.Requests;
using Structur.Server.Responses;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Structur.Server
{
    public class IOController : WebApiController
    {
        public const string ControllerInputName = "/key";
        public const string ControllerHistoryeName = "/history";
        public const string ControllerVersionName = "/version";

        private const string PostProgramInputErrorText = "Server Error: The server did not recognize the key request body as valid input...";
        private readonly StructureIO _io;
        private readonly QueuedInput _serverInput;

        public IOController(StructureIO io, QueuedInput serverInputQueue)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _serverInput = serverInputQueue ?? throw new ArgumentNullException(nameof(serverInputQueue));
        }

        [Route(HttpVerbs.Get, ControllerInputName)]
        public InputResponse GetScreen()
        {
            return InputResponse.Create(_io.CurrentDisplay);
        }

        [Route(HttpVerbs.Post, ControllerHistoryeName)]
        public async Task<HistoryResponse> GetHistoryAsync()
        {
            var request = await HttpContext.GetRequestDataAsync<HistoryRequest>();
            var history = new List<ProgramInputData>();
            var sessions = CreateInputsFromSavedSessions();
            var counter = 0;

            while (sessions.Count != 0 && counter + sessions[0].NumberOfInputs < request.StartInclusive)
            {
                counter += sessions[0].NumberOfInputs;
                sessions.RemoveAt(0);
            }
            while (sessions.Count != 0 && sessions[0].IsInputAvailable() && sessions[0].NumberOfInputs < request.StartInclusive) 
            {
                counter += 1;
                sessions[0].ReadInput();
            }

            while (counter < request.StopExclusive && sessions.Count > 0)
            {
                if (sessions[0].IsInputAvailable())
                {
                    counter++;
                    history.Add(sessions[0].ReadInput());
                }
                else sessions.RemoveAt(0);
            }

            return HistoryResponse.Create(history.ToArray());
        }

        private static IList<PredeterminedInput> CreateInputsFromSavedSessions()
        {
            var savedDataSessions = SavedSessionUtilities.LoadSavedDataSessions();
            var sessionsInputs = savedDataSessions.Select(x => new PredeterminedInput(x));
            return sessionsInputs.ToList();
        }

        [Route(HttpVerbs.Post, ControllerInputName)]
        public async Task<InputResponse> PostProgramInputAsync()
        {
            var inputData = await HttpContext.GetRequestDataAsync<ProgramInputData>();
            if (inputData == null) return ErrorInputResponse();

            using (WaitUntilOutputIsIdle()) _serverInput.EnqueueInput(inputData);

            return InputResponse.Create(_io.CurrentDisplay);
        }

        [Route(HttpVerbs.Get, ControllerVersionName)]
        public VersionResponse GetVersion()
        {
            var version = StructureProgram.Version;
            return new VersionResponse
            {
                Major = version.Major,
                Minor = version.Minor,
                Build = version.Build,
                KeyHash = _io.KeyHash.ToString(CultureInfo.CurrentCulture),
                KeyCount = _io.KeyCount,
            };
        }

        private DisposableAction WaitUntilOutputIsIdle()
        {
            _io.IsBusy = true;
            return new DisposableAction(() =>
            {
                while(_io.IsBusy) { };
            });
        }

        private static InputResponse ErrorInputResponse()
        {
            return new InputResponse
            {
                ConsoleText = PostProgramInputErrorText
            };
        }
    }
}