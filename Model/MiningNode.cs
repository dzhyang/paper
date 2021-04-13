using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Model
{
    public class MiningNode
    {
        public char Feature;
        public double Ppt;
        public Dictionary<char, MiningNode> Children;
    }
}
