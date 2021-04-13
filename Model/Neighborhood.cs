using System;
using System.Collections.Generic;
using System.Text;

namespace Paper.Model
{
    internal sealed class Neighborhood
    {
        public Neighborhood()
        {
            NeighborhoodWithFeature = new Dictionary<char, List<Instance>>();
        }
        public Instance Instance { get; set; }
        public Dictionary<char, List<Instance>> NeighborhoodWithFeature { get; set; }

        public override string ToString() => this.Instance.ToString()+ " Neighborhood Count "+NeighborhoodWithFeature.Count;
    }
}