using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Extensions.Interfaces
{
    public interface IDataOperator<TRead,TWrite>
    {

        TRead Read(string filepath);
        void Write(string filePath, TWrite data);
    }
}
