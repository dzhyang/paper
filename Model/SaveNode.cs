using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paper.Model
{
    class SCPNode
    {
        private readonly object _locknextobject = new();


        //private string _features;

        //public ConcurrentBag<SaveNode> Supper { get; set; }

        public string Features { get; init; }

        public double Prev { get; init; }

        public bool IsUsed { get; set; }

        public ConcurrentBag<SCPNode> Next { get; init; }

        /// <summary>
        /// 添加，线程安全
        /// </summary>
        /// <param name="node">模式</param>
        public void AddNext(SCPNode node)
        {
            lock (_locknextobject)
            {
                if (!Next.Contains(node)) Next.Add(node);
            }
        }



        public override int GetHashCode() => Features.GetHashCode();

        public override bool Equals(object obj) => obj is SCPNode oj && Features == oj.Features;
    }
    
}
