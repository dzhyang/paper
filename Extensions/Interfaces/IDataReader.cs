using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Extensions.Interfaces
{
    public interface IDataReader<T>
    {
        T Read(string filepath);
    }
}
