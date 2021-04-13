using Paper.Extensions;
using Paper.Log;
using Paper.Model;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Paper.Co_location
{
    internal class CoLocationBuilder
    {
        // 源数据
        private readonly Dictionary<char, List<Instance>> _sourceData;

        private readonly Dictionary<char, int> _featureInsCount;

        /// <summary>
        /// 存储特征及其实例邻居
        /// </summary>
        private readonly Dictionary<char, Dictionary<short, Neighborhood>> _neighborhoods;

        /// <summary>
        /// 存储所有团实例
        /// </summary>
        private readonly Dictionary<int, List<string>> _dictOfPatternWithLen;

        /// <summary>
        /// 不同特征全局id与逻辑id映射
        /// 生成当前阶的模式时查询使用
        /// </summary>
        private Dictionary<char, Dictionary<short, short>> _idMapping;


        // 5  8
        private Dictionary<ValueType, HashSet<ValueType>> _beforeArea;

        private Dictionary<ValueType, HashSet<ValueType>> _currentArea;




        private readonly Dictionary<string, double> _colocationPatterns;

        private List<ConcurrentBag<short>> _useFeatureCount;

        #region initial

        private readonly Configuration _config;
        private readonly Logger _log;
        private readonly DataAdapter _dataAdapter;


        public CoLocationBuilder(Configuration configuration, Logger logger,DataAdapter dataAdapter)
        {
            _log = logger;
            _config = configuration;
            _dataAdapter = dataAdapter;
            Dictionary<char, List<Instance>> data = dataAdapter.Read();

            Cluster(data);
            _sourceData = data;

            _dictOfPatternWithLen = new Dictionary<int, List<string>>()
            {
                {1,data.Keys.Select(feature=>feature.ToString()).ToList()}
            };

            _neighborhoods = new Dictionary<char, Dictionary<short, Neighborhood>>(_sourceData.Keys.Take(_sourceData.Keys.Count - 1).Select(key => new KeyValuePair<char, Dictionary<short, Neighborhood>>(key, new Dictionary<short, Neighborhood>())));

            _featureInsCount = new Dictionary<char, int>(data.Keys.Select(key => new KeyValuePair<char, int>(key, data[key].Count)));

            _colocationPatterns = new Dictionary<string, double>();

            InitialNeighbor();
        }

        private void Cluster(Dictionary<char, List<Instance>> data)
        {
            var temp = new List<Instance>();
            foreach (var feature in data.Keys.ToArray())
            {
                while (data[feature].Count != 0)
                {

                    var mainpoint = data[feature][data[feature].Count / 2];

                    //范围阈值越大，去除数据量越大
                    data[feature].RemoveAll(l => l.IsApproaching(mainpoint, 0.008));
                    data[feature].Remove(mainpoint);
                    temp.Add(mainpoint);
                }
                data[feature] = new List<Instance>(temp);
                _log.LogInfo($"{feature}:{temp.Count}");
                temp.Clear();
            }
        }



        private void InitialNeighbor()
        {
            Expand.RunTime(() =>
            {
                foreach (var beforeKey in _sourceData.Keys)
                {
                    _log.LogInfo($"开始计算{{{beforeKey}}}的星形邻居");
                    foreach (var beforeItem in _sourceData[beforeKey])
                    {
                        var afterKeys = _sourceData.Keys.Where(key => key > beforeKey);
                        if (afterKeys.Count() == 0)
                        {
                            break;
                        }

                        var neighborhood = new Neighborhood()
                        {
                            Instance = beforeItem
                        };
                        foreach (var afterKey in afterKeys)
                        {
                            neighborhood.NeighborhoodWithFeature.Add(afterKey,
                                _sourceData[afterKey].Where(afterItem => beforeItem.IsApproaching(afterItem, _config.DistanceThreshold)).ToList()); /*ToDictionary(inst => inst.Id));*/
                        }
                        _neighborhoods[beforeKey].Add(beforeItem.Id, neighborhood);
                    }
                }
            }, "生成星形邻居耗时{0}");
        }

        #endregion

        public void BuildColocation()
        {

            
            for (int i = 2; i < _featureInsCount.Keys.Count; i++)
            {
                _log.LogInfo($"开始{i}阶模式计算");
                _beforeArea = _currentArea;
                _currentArea = new Dictionary<ValueType, HashSet<ValueType>>();
                BuildColocation(i);
                _log.LogInfo($"结束{i}阶模式计算\n");
            }
            _dataAdapter.Write(_colocationPatterns);
           
        }

        private void BuildColocation(int index)
        {

            var currentStageIdMapping = new Dictionary<char, Dictionary<short, short>>();
            var maxLogicId = new Dictionary<char, short>();
            var maxLogicIdCopy = new Dictionary<char, short>();
            _dictOfPatternWithLen.Add(index, new List<string>());
            foreach (var pattern in CombinationNoRecursive(_dictOfPatternWithLen[index - 1]))
            {
                // 临时映射表
                var tempIdMapping = new Dictionary<char, Dictionary<short, short>>();

                //_log.LogInfo($"开始计算{pattern}是否频繁");

                //当前使用的id,若当前模式可行，则加入映射表中。
                Dictionary<char, IEnumerable<short>> idUsed = new Dictionary<char, IEnumerable<short>>();
                (bool isPass, double prev) = FilterNeighborhood(pattern, currentStageIdMapping, ref idUsed);
                if (!isPass)
                {
                    _log.LogError($"模式{pattern}未通过参与度一级过滤");
                    continue;
                }

                foreach (var feature in pattern)
                {
                    if (!maxLogicId.ContainsKey(feature))
                    {
                        maxLogicId.Add(feature, -1);
                    }

                    short max = maxLogicId[feature];
                    tempIdMapping.Add(feature,
                        idUsed[feature].ToDictionary(id => id, id =>
                           {
                               max += 1;
                               return max;
                           })
                        );
                    maxLogicId[feature] = max;
                }

                if (pattern.Length == 2)//二阶只要参与度达标，就可以不执行之后的成团筛选
                {
                    //添加映射
                    currentStageIdMapping.Merge(tempIdMapping);
                    //保存结果
                    _log.LogInfo($"{pattern}：{prev}");
                    _colocationPatterns.Add(pattern, prev);
                    _dictOfPatternWithLen[index].Add(pattern);

                    if (pattern[0] == 'A')
                    {
                        continue;
                    }

                    //存储实例
                    foreach (var neighborhood in _neighborhoods[pattern[0]].Values)
                    {
                        //首特征id映射
                        short firstLogicId = currentStageIdMapping[neighborhood.Instance.Feature][neighborhood.Instance.Id];

                        byte firstInstArea = (byte)(firstLogicId >> 8);
                        //第二特征id映射
                        var neighbor = pattern.Skip(1).Select(feature => neighborhood.NeighborhoodWithFeature[feature]
                                                                        .Select(inst => currentStageIdMapping[inst.Feature][inst.Id]));
                        foreach (var product in neighbor.First())
                        {
                            byte secondArea = (byte)(product >> 8);
                            var area = ValueTuple.Create(firstInstArea, secondArea);
                            if (!_currentArea.ContainsKey(area))
                            {
                                _currentArea.Add(area, new HashSet<ValueType>());
                            }

                            _currentArea[area].Add(ValueTuple.Create((byte)firstLogicId, (byte)product));
                        }
                    }
                    maxLogicIdCopy = new Dictionary<char, short>(maxLogicId);
                    continue;
                }

                _useFeatureCount = new List<ConcurrentBag<short>>();
                pattern.All(feature =>
                {
                    _useFeatureCount.Add(new ConcurrentBag<short>());
                    return true;
                }
                );
                var mappingParam = new Dictionary<char, Dictionary<short, short>>();
                mappingParam.Merge(currentStageIdMapping);
                mappingParam.Merge(tempIdMapping);

                (bool _isPass, double _prev) = CartesianProduct(pattern, mappingParam);


                if (_isPass)
                {
                    //id映射存储
                    currentStageIdMapping.Merge(tempIdMapping);
                    _log.LogInfo($"{pattern}：{_prev}");
                    _colocationPatterns.Add(pattern, _prev);
                    _dictOfPatternWithLen[index].Add(pattern);
                    maxLogicIdCopy = new Dictionary<char, short>(maxLogicId);
                }
                else
                {
                    tempIdMapping = null;
                    maxLogicId = new Dictionary<char, short>(maxLogicIdCopy);
                    _log.LogError($"模式{pattern}未通过参与度过滤:{_prev}");
                }
            }
            _idMapping = currentStageIdMapping;
        }

        // 由备选模式获取其实例
        private (bool isPass, double prev) FilterNeighborhood(string pattern, Dictionary<char, Dictionary<short, short>> mapping, ref Dictionary<char, IEnumerable<short>> idDic)
        {
            // 内部集合为同一特征的实例
            List<HashSet<Instance>> stars = new List<HashSet<Instance>>();//  换为集
            foreach (var neighborhood in _neighborhoods[pattern.Min()].Values)
            {
                if (stars.Count == 0)
                {
                    stars.Add(new HashSet<Instance>() { neighborhood.Instance });
                    stars.AddRange(pattern.Skip(1).Select(feature => neighborhood.NeighborhoodWithFeature[feature].ToHashSet()));
                }
                else
                {
                    stars[0].Add(neighborhood.Instance);
                    using (var starEnumerator = stars.Skip(1).GetEnumerator())
                    using (var patternEnumerator = pattern.Skip(1).GetEnumerator())
                    {
                        while (starEnumerator.MoveNext() && patternEnumerator.MoveNext())
                        {
                            foreach (var item in neighborhood.NeighborhoodWithFeature[patternEnumerator.Current])
                            {
                                starEnumerator.Current.Add(item);
                            }

                        }
                    }
                }
            }
            idDic = stars.Select(use => (use.FirstOrDefault()?.Feature ?? '0', use.Where(inst => mapping.ContainsKey(inst.Feature) ?
                                                                                  (!mapping[inst.Feature].ContainsKey(inst.Id)) : true)
                                                                                  .Select(inst => inst.Id)))
                         .ToDictionary(tuple => tuple.Item1, tuple => tuple.Item2);

            return Prevalent(
                (featureCount: pattern.Select(feature => _featureInsCount[feature]), useCount: stars.Select(star => star.Count)),
                _config.MinPrev);
        }


        /* 
         * 1.文件(?)存储每一阶模式的成团实例。
         * 2.建立全局Id与虚拟Id的映射，映射时应遵循二进制下空闲位最少原则。
         * 3.全局Id由算法生成index。------------------------------------>index生成算法：
         * 4.存储方式为二进制，对应index的位为对应实例是否成团。
         * 5.查找时使用内存映射
         * 
         * 
         * !!!!!!!!!!!
         * 以第一个元素为首的不用缓存！！！
         * 
         * index生成算法
         * 1.A不参与缓存
         * 2.不成团的实例不存储
         * 3.问题在于如何编号保证只存储成团实例，
         * 
         * 5000 2^13
         *xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
         *x 13位二进制，前8位为前缀，后5位为后缀，此时有一个外部映射表，该表长度为2^8，即2的前缀长度次的幂，为256，表元素是一个列表，索引为后缀，列表元素为字
         *x 典，键为特征，值一个hashset，
         *x 存储序列的下一个特征的前缀，
         *x 
         *x Dictionary<char,Dictionary<char, HashSet<byte>>[]>
         *x 
         *x 压缩存入时，现在有序数对为<4096，4097，4098，4099，4100，5000>，使用一个32位数保存，那么该32位数保存后缀，六个值需要30位，足够用。保存每个数时，
         *x 先取八位前缀，在本例中，前五个值前缀均为100000000，那就是映射表128号，得到对应字典，向字典的对应键的值的set中添加下一个特征的前缀码。首个特征的前
         *x 缀码存入最后一个后缀码对应的set中。
         *
         *x 存入哈希表。
         *x
         *x 判断有序数对是否存在时，先组合后缀，查找哈希表，查看由后缀组合的值是否存在，不存在就不用去再找映射表，可进行一次剪枝；若存在，则查找映射表对应前缀位
         *x 置元素对应字典的键值下的set中是否有下个特征的前缀码。
         *x
         *x 空间占用：若不压缩，则需使用64位存储有序数对，空间翻倍，若压缩，空间多了映射表的空间，映射表空间占用是256×32＝8192bit，就是1kb。
         *x 时间效率：由于映射表查找是使用索引，时间效率就是O(1)，影响不大。
         *x
         *xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
         */


        private (bool _isPass, double _prev) CartesianProduct(string pattern, Dictionary<char, Dictionary<short, short>> idMapping)
        {
            var currentStageArea = new Dictionary<ValueType, HashSet<ValueType>>();

            var usedInst = pattern.Select(feature => feature).ToDictionary(feature => feature, feature => new HashSet<short>());


            foreach (var neighborhood in _neighborhoods[pattern[0]].Values)
            {
                //首特征的邻居集合
                var neighbor = pattern.Skip(1).Select(feature => neighborhood.NeighborhoodWithFeature[feature]
                                                                        .Select(inst => inst.Id));
                var result = new List<List<short>>();
                Expand.Descartes(neighbor.ToList(), result);
                foreach (var product in result)
                {
                    //判断product是否成团。id映射为上一阶的逻辑id
                    //不成团直接返回
                    var (areaId, indId) = Expand.CreateId(product.Select((gId, i) => _idMapping[pattern[i + 1]][gId]).ToArray());
                    if ((_beforeArea.ContainsKey(areaId)) && (_beforeArea[areaId].Contains(indId)))
                    {
                        continue;
                    }
                    //参与集
                    var i = 0;
                    usedInst[pattern[0]].Add(neighborhood.Instance.Id);
                    foreach (var gId in product)
                    {
                        usedInst[pattern[i + 1]].Add(gId);
                        i += 1;
                    }
                    //成团，若neighborhood特征不为A，则存，为A则不存  id映射为当前阶逻辑id
                    if (pattern[0] == 'A')
                    {
                        continue;
                    }

                    var (newAreaId, newIndId) = Expand.CreateId(new short[] { idMapping[neighborhood.Instance.Feature][neighborhood.Instance.Id] }
                    .Concat(product.Select((gId, i) => idMapping[pattern[i + 1]][gId])).ToArray());

                    if (!currentStageArea.ContainsKey(newAreaId))
                    {
                        currentStageArea.Add(newAreaId, new HashSet<ValueType>());
                    }

                    currentStageArea[newAreaId].Add(newIndId);
                }
            }

            //参与度判断
            (bool isPass, double prev) = Prevalent((featureCount: pattern.Select(feature => _featureInsCount[feature]), useCount: usedInst.Select(kv => kv.Value.Count)), _config.MinPrev);
            if (isPass)
            {
                foreach (var (key, values) in currentStageArea)
                {
                    if (!_currentArea.ContainsKey(key))
                    {
                        _currentArea.Add(key, values);
                    }
                    var tempSet = _currentArea[key];
                    foreach (var value in values)
                    {
                        tempSet.Add(value);
                    }
                }

            }
            return (isPass, prev);

        }

        #region 组合及参与度计算算法

        //由k-1阶模式生成k阶
        private IEnumerable<string> CombinationNoRecursive(IEnumerable<string> patterns)
        {
            int len = patterns.FirstOrDefault()?.Length ?? 0;
            //k-1阶模式中前k-2个特征相同的两模式组合为一个k阶模式
            return patterns.SelectMany(patternbefore => patterns.Where(patternafter =>
            (patternbefore.Last() < patternafter.Last()) && (patternbefore.Substring(0, len - 1) == patternafter.Substring(0, len - 1))),
            (patternbefore, patternafter) => patternbefore + patternafter.Last()).AsEnumerable();
        }

        private (bool, double) Prevalent((IEnumerable<int> featureCount, IEnumerable<int> useCount) count, double threshold)
        {
            var prev = Enumerable.Min(Enumerable.Zip(count.featureCount, count.useCount, (fc, uc) => uc / (double)fc));
            return (prev > threshold, prev);
        }

        #endregion
    }
}
