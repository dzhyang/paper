using Paper.Extensions.Interfaces;
using Paper.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paper.Extensions
{
    public class OriginalDataReader : IDataReader<Dictionary<char, List<Instance>>>
    {
        public Dictionary<char,List<Instance>> Read(string filepath)
        {
            var result = (from source in File.ReadAllLines(filepath)
                          let temp = source.Split(',')
                          select new Instance() { Id = Convert.ToInt16(temp[0]), Feature = Convert.ToChar(temp[1]), X = Convert.ToSingle(temp[2]), Y = Convert.ToSingle(temp[3]) }).GroupBy(l => l.Feature).OrderBy(l => l.Key);
            
            return result.ToDictionary(l => l.Key, l => l.ToList());

            //return result.ToDictionary(l => l.Key, l => l.Select((ins, index) =>
            //{
            //    ins.IdInFeature = Convert.ToInt16(index); 
            //    return ins;
            //}).ToList());
        }
    }
}
