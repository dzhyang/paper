using Paper.Extensions.Interfaces;
using Paper.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paper.Extensions
{
    public class ColocationDataReader : IDataReader<Dictionary<string,double>> 
    {
        public Dictionary<string, double> Read(string filepath)
        {
            var temp = (from line in File.ReadAllLines(filepath)
                        let src = line.Split(',')
                        select (src[0], src[1])).OrderBy(l => l.Item1);
            var rt = new Dictionary<string, double>();
            foreach (var item in temp)
            {
                var part = Convert.ToDouble(item.Item2);
                if (rt.TryAdd(item.Item1, part)) continue;

                if (rt[item.Item1] < part) rt[item.Item1] = part;
            }
            return rt;
        }
    }
}
