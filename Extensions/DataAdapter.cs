using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Paper.Extensions
{
    public enum DataType
    {
        OriginalData2CoLocationData,
        CoLocationData2ESCData,
    }
    public class DataAdapter
    {

        private readonly Type[] _dataOperatorTypes=new Type[] { 
            typeof(OriginalDataOperator),
            typeof(CoLocationDataOperator)
        };

        private readonly dynamic _dataOperator;
        private readonly string _readPath;
        private readonly string _writePath;
        public DataAdapter(string[] fileNames, Configuration configuration, DataType dataType)
        {
            var dataTypes = dataType.ToString().Split("2");
            _readPath = configuration[dataTypes[0]];
            _writePath = configuration[dataTypes[1]];
            _dataOperator = Activator.CreateInstance(_dataOperatorTypes[(int)dataType], new object[] { fileNames });
        }


        public dynamic Read()
        {
            return _dataOperator.Read(_readPath);
        }

        public void Write(dynamic data)
        {
            _dataOperator.Write(_writePath,data);
        }
    }
}
