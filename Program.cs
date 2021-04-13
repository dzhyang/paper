using Paper.Co_location;
using Paper.Extensions;
using Paper.Log;

using System;
namespace Paper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = new Configuration();
            var log = new Logger(new ConsoleLogger());

            var dataAdapter = new DataAdapter("2011_clear",config,DataType.OriginalData2CoLocationData);
            CoLocationBuilder builder = new(config, log, dataAdapter);
            builder.BuildColocation();
            Console.WriteLine("!!!");
        }
    }
}
