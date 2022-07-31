using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Structur.IO;
using Structur.IO.Input;
using Structur.Program;
using System;

namespace Structur.Server
{
    internal class IOController : WebApiController
    {
        public const string ControllerName = "key";

        private readonly StructureIO _io;

        public QueuedInput ServerInputQueue { get; set; }

        public IOController(StructureIO io, QueuedInput queuedInput)
        {
            _io = io;
            ServerInputQueue = queuedInput;
        }

        [Route(HttpVerbs.Get, "/" + ControllerName)]
        public InputResponse GetScreen()
        {
            var response = new InputResponse();
            response.ConsoleText = _io.CurrentBuffer.ToString();
            return response;
        }

        [Route(HttpVerbs.Post, "/" + ControllerName)]
        public InputResponse EnterKey()
        {
            var programInputData = HttpContext.GetRequestDataAsync<ProgramInputData>()?.Result;
            if (programInputData != null)
            {
                _io.IsBusy = true;
                ServerInputQueue?.EnqueueKey(programInputData);
                while (_io.IsBusy) { }
                var response = new InputResponse();
                response.ConsoleText = _io.CurrentBuffer?.ToString();
                return response;
            }
            return new InputResponse { ConsoleText = "Server Error: The server did not recognize the key request body as valid input..."};
        }

        [Route(HttpVerbs.Get, "/" + "version")]
        public VersionResponse GetVersion()
        {
            var response = new VersionResponse
            {
                Major = StructureProgram.Version.Major,
                Minor = StructureProgram.Version.Minor,
                Build = StructureProgram.Version.Build,
                KeyHash = _io.KeyHash,
                KeyCount = _io.KeyCount,
            };
            return response;
        }
    }
}