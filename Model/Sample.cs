using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class Sample {
        [DataMember] [ProtoMember(3)] public List<int> BufferSizes;

        [DataMember] [ProtoMember(2)] public List<float> MaterialRatios;

        [DataMember] [ProtoMember(1)] public List<float> ProcessingRates;

        [DataMember] [ProtoMember(4)] public float ProductionRate;

        public override string ToString() {
            var bsizes = string.Join(",", BufferSizes);
            var prates = string.Join(",", ProcessingRates);
            var mratios = string.Join(",", MaterialRatios);
            return
                $"BufferSizes: {bsizes}; MaterialRatios: {mratios}; ProcessingRates: {prates}; ProductionRate: {ProductionRate}";
        }

        public List<float> ToFloats() {
            return ProcessingRates.Concat(MaterialRatios).Concat(BufferSizes.Select(b => (float) b)).ToList();
        }
    }
}