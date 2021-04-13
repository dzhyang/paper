using Paper.Model;

using System.Collections.Generic;

namespace Paper.Co_location
{
    internal interface IVisitor<T>
    {
        internal T Take(); 
    }
}