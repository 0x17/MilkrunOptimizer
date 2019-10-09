using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class Sample {
        [DataMember] [ProtoMember(1)] public List<float> ProcessingRates;
        
        [DataMember] [ProtoMember(2)] public List<int> OrderUpToLevels;

        [DataMember] [ProtoMember(3)] public List<int> BufferSizes;
        
        [DataMember] [ProtoMember(4)] public int MilkrunCycleLength;

        [DataMember] [ProtoMember(5)] public float ProductionRate;

        public override string ToString() {
            var prates = string.Join(",", ProcessingRates);
            var oul = string.Join(",", OrderUpToLevels);
            var bsizes = string.Join(",", BufferSizes);
            return
                $"BufferSizes: {bsizes}; MaterialRatios: {oul}; ProcessingRates: {prates}; MilkrunCycleLength: {MilkrunCycleLength}; ProductionRate: {ProductionRate}";
        }

        public List<float> ToFloats() {
            return ProcessingRates.Concat(OrderUpToLevels.Select(oul => (float)oul)).Concat(BufferSizes.Select(b => (float) b)).Append(MilkrunCycleLength).ToList();
        }
    }
}