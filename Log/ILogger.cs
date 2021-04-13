using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Log.Private
{
    interface ILogger
    {
        void LogInfo(string message);

        void LogError(string message);
    }
}
