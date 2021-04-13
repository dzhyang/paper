using Paper.Extensions.Interfaces;
using Paper.Log;
using Paper.Model;

using System;
using System.Collections.Generic;

using System.Diagnostics;
using System.Linq;

namespace Paper.Extensions
{
    public static class Expand
    {
        private static readonly Logger _logger;
        static Expand()
        {
            _logger = new Logger(new ConsoleLogger());
        }
        public static void RunTime(Action action, string message)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            _logger.LogInfo(string.Format(message, stopwatch.Elapsed.TotalSeconds));
        }

        //public static TOutType ReadData<TOutType,TReader>(string filepath) where TReader : IDataOperator<TOutType>, new()
        //{
        //    IDataOperator<TOutType> reader = new TReader();
        //    //　TODO 文件路径检查
        //    //PathCheck(filepath);
        //    return reader.Read("");
        //}

        /// <summary>
        /// 判断实例是否临近
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        public static bool IsApproaching(this Instance first, Instance second, double threshold)
        {
            return Math.Sqrt(Math.Pow(Math.Abs(first.X - second.X), 2) + Math.Pow(Math.Abs(first.Y - second.Y), 2)) < threshold;
        }

        public static void Merge<TKey, TValue>(this Dictionary<TKey,TValue> baseKeyValues, Dictionary<TKey, TValue> mergeValue)
            where TValue:IDictionary<short,short>,new()
        {
            foreach (var (key, values) in mergeValue)
            {
                if (!baseKeyValues.ContainsKey(key))
                {
                    baseKeyValues.Add(key, new TValue());
                }
                var temp = baseKeyValues[key];
                foreach (var value in values)
                {
                    temp.Add(value);
                }
            }
        }
        
        public static (ValueType,ValueType) CreateId(params short[] id)
        {
            var areaId=id.Select(p => (byte)(p >> 8)).ToList();
            var indId = id.Select(p => (byte)p).ToList();

            return areaId.Count switch
            {
                2 => (ValueTuple.Create(areaId[0], areaId[1]),
                      ValueTuple.Create(indId[0], indId[1])),
                3 => (ValueTuple.Create(areaId[0], areaId[1], areaId[2]),
                      ValueTuple.Create(indId[0], indId[1], indId[2])),
                4 => (ValueTuple.Create(areaId[0], areaId[1], areaId[2], areaId[3]),
                      ValueTuple.Create(indId[0], indId[1], indId[2], indId[3])),
                5 => (ValueTuple.Create(areaId[0], areaId[1], areaId[2], areaId[3], areaId[4]),
                      ValueTuple.Create(indId[0], indId[1], indId[2], indId[3], indId[4])),
                6 => (ValueTuple.Create(areaId[0], areaId[1], areaId[2], areaId[3], areaId[4], areaId[5]),
                      ValueTuple.Create(indId[0], indId[1], indId[2], indId[3], indId[4], indId[5])),
                7 => (ValueTuple.Create(areaId[0], areaId[1], areaId[2], areaId[3], areaId[4], areaId[5], areaId[6]),
                      ValueTuple.Create(indId[0], indId[1], indId[2], indId[3], indId[4], indId[5], indId[6])),
                _ => (0,0),
            };
        }

        public static IEnumerable<IEnumerable<T>> Cartesian<T>(IEnumerable<IEnumerable<T>> source)
        {
            IEnumerable<IEnumerable<T>> seed = new[] { Enumerable.Empty<T>() };
            return source.Aggregate(seed, (seed, next) => seed.SelectMany(_ => next, (seed, next) => seed.Concat(new[] { next })));
        }
        public static void Descartes(List<IEnumerable<short>> source, List<List<short>> result,short index = 0, List<short> data = default)
        {

            if (data == null) data = new List<short>();
            //var tempsource = new List<IEnumerable<short>>(source);
            foreach (var id in source[index])
            {
                data.Add(id);
                if ((index + 1) < source.Count)
                {
                    Descartes(source,result,(short)(index + 1), data);
                }
                else 
                {
                    result.Add(new List<short>(data));
                }
                data.RemoveAt(data.Count-1);
            }
        }

    }
}
