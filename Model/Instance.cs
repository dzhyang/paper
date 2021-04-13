using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Model
{
    /// <summary>
    /// 实例
    /// </summary>
    public class Instance
    {
        /// <summary>
        /// 实例唯一索引
        /// </summary>
        public short Id { get; set; }
        /// <summary>
        /// 特征
        /// </summary>
        public char Feature { get; set; }

        /// <summary>
        /// 特征集内索引
        /// </summary>
        //public short IdInFeature { get; set; }

        /// <summary>
        /// 邻居集内索引
        /// </summary>
        //public short IndexInNeighborhoods { get; set; }

        /// <summary>
        /// X坐标
        /// </summary>
        public float X { get; set; }
        /// <summary>
        /// Y坐标
        /// </summary>
        public float Y { get; set; }

        // 12^20 feature^id
        public override int GetHashCode() => (Feature << 20) ^ Id.GetHashCode();

        public override string ToString() => string.Concat(Feature,"-",Id);
        // 16^16 feature^id
        //public override int GetHashCode() => (Feature<<16)^IdInFeature.GetHashCode();

        //public new long GetHashCode() => ((long)Feature.GetHashCode()<<32)^Id.GetHashCode();
    }
}
