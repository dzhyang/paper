using Paper.Extensions.Interfaces;
using Paper.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Paper.Extensions
{
    public class OriginalDataOperator : IDataOperator<Dictionary<char, List<Instance>>,Dictionary<string,double>>
    {

        private int seek=0;
        private readonly string[] fileNames;

        public OriginalDataOperator(string[] fileNames)
        {
            this.fileNames = fileNames;
        }


        
        public Dictionary<char,List<Instance>> Read(string filepath)
        {
            if (seek == fileNames.Length) return null;
            filepath += fileNames[seek];
            var result = (from source in File.ReadAllLines(filepath)
                          let temp = source.Split(',')
                          select new Instance()
                          {
                              Id = Convert.ToInt16(temp[0]), 
                              Feature = Convert.ToChar(temp[1]), 
                              X = Convert.ToSingle(temp[2]), 
                              Y = Convert.ToSingle(temp[3]) 
                          }).GroupBy(l => l.Feature).OrderBy(l => l.Key);
            seek += 1;
            return result.ToDictionary(l => l.Key, l => l.ToList());
            
            //return result.ToDictionary(l => l.Key, l => l.Select((ins, index) =>
            //{
            //    ins.IdInFeature = Convert.ToInt16(index); 
            //    return ins;
            //}).ToList());
        }

        public void Write(string filePath,Dictionary<string, double> data)
        {
            using (StreamWriter writer = new(filePath, true))
            {
                foreach (var (key, value) in data)
                {
                    writer.WriteLine(key + "," + value);
                }
            }
        }
    }
}
