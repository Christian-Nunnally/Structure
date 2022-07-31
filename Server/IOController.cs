using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Structur.IO;
using Structur.IO.Input;
using Structur.Program;
using Structur.Program.Utilities;
using System;
using System.Threading.Tasks;

namespace Structur.Server
{
    internal class IOController : WebApiController
    {
        public const string ControllerName = "/key";
        public const string ControllerVersionName = "/version";

        private const string PostProgramInputErrorText = "Server Error: The server did not recognize the key request body as valid input...";
        private readonly StructureIO _io;
        private readonly QueuedInput _serverInput;

        public IOController(StructureIO io, QueuedInput serverInputQueue)
        {
            _io = io ?? throw new ArgumentNullException(nameof(io));
            _serverInput = serverInputQueue ?? throw new ArgumentNullException(nameof(serverInputQueue));
        }

        [Route(HttpVerbs.Get, ControllerName)]
        public InputResponse GetScreen()
        {
            return InputResponse.Create(_io.CurrentDisplay);
        }

        [Route(HttpVerbs.Post, ControllerName)]
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
                KeyHash = _io.KeyHash,
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