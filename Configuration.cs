using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace Paper
{
    public class Configuration
    {
        /// <summary>
        /// 原始数据文件目录
        /// </summary>
        public string OriginalDataFolder { get; }
        /// <summary>
        /// co-location模式存储目录
        /// </summary>
        public string CoLocationDataFolder { get; }
        /// <summary>
        /// 随机生成测试数据集目录
        /// </summary>
        public string TestDataFolder { get; }
        /// <summary>
        /// 结果目录
        /// </summary>
        public string ESCDataFolder { get; }
        /// <summary>
        /// 缓存目录
        /// </summary>
        public string CacheFolder { get; }
        /// <summary>
        /// top-k
        /// </summary>
        public int Topk { get; set; }
        /// <summary>
        /// 最小参与度
        /// </summary>
        public float MinPrev { get; set; }
        /// <summary>
        /// 距离阈值
        /// </summary>
        public float DistanceThreshold { get; set; }

        /// <summary>
        /// 队列最大容量
        /// </summary>
        public int QueueMaxCapacity { get; set; }

        /// <summary>
        /// 任务最大容量
        /// </summary>
        public int TaskMaxCount { get; set; }


        private readonly Dictionary<string, string> _cacheDictionary;

        public string this[string key] { get
            {
                return _cacheDictionary[key];
            } }
        public Configuration()
        {
            this.OriginalDataFolder = "../OriginalData/";
            this.CoLocationDataFolder = "../CoLocationData/";
            this.TestDataFolder = "../TestData/";
            this.ESCDataFolder = "../ESCData/";

            this.CacheFolder = "../Cache/";

            this.Topk = 0F;
            this.MinPrev = 0.5F;
            this.DistanceThreshold = 0.03F;
            this.QueueMaxCapacity = 10000;
            this.TaskMaxCount = 1;

            var configType = GetType();
            var folders = configType.GetProperties()
                .Where(info => info.Name.EndsWith("Folder"))
                .ToDictionary(info => info.Name.Replace("Folder", ""), info => info.GetValue(this) as string);
            configType.GetField("_cacheDictionary", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(this, folders);
        }
    }
}
