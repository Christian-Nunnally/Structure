using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Structure.IO;
using Structure.IO.Input;
using System;

namespace Structure.Server
{
    internal class Controller : WebApiController
    {
        public const string ControllerName = "key";

        private readonly StructureIO _io;

        public QueuedInput ServerInputQueue { get; set; }

        public Controller(StructureIO io, QueuedInput queuedInput)
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
            try
            {
                var programInputData = HttpContext.GetRequestDataAsync<ProgramInputData>().Result;
                if (programInputData != null)
                {
                    _io.IsBusy = true;
                    ServerInputQueue?.EnqueueKey(programInputData);
                    while (_io.IsBusy) { }
                    var response = new InputResponse();
                    response.ConsoleText = _io.CurrentBuffer.ToString();
                    return response;
                }
                throw new InvalidOperationException();
            }
            catch
            {
                return new InputResponse();
            }
        }
    }
}