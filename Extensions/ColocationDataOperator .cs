using Paper.Extensions.Interfaces;
using Paper.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Paper.Extensions
{
    public class CoLocationDataOperator : IDataOperator<Dictionary<string,double>, Dictionary<string, double>>
    {
        private int seek = 0;
        private readonly string[] fileNames;

        public CoLocationDataOperator(string[] fileNames)
        {
            this.fileNames = fileNames;
        }

        public Dictionary<string, double> Read(string filepath)
        {
            if (seek == fileNames.Length) return null;
            filepath += fileNames[seek];

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
            seek += 1;
            return rt;
        }

        public void Write(string filePath, Dictionary<string, double> data)
        {
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (var (key, value) in data)
                {
                    writer.WriteLine(key + "," + value);
                }
            }
        }
    }
}
