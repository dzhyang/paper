using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Log
{
    public class ConsoleLogger : Private.ILogger
    {
        public void LogError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[Error] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
        public void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("[Info] ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(message);
        }
    }
}
