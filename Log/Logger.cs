using Paper.Log.Private;

using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Log
{
    class Logger
    {
        private readonly ILogger _logger;

        public Logger(ILogger logger) => this._logger = logger;

        public void LogError(string message) => _logger.LogError(message);
        public void LogInfo(string message) => _logger.LogInfo(message);
    }
}
