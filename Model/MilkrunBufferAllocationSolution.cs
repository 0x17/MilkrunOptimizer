using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class MilkrunBufferAllocationSolution {
        [DataMember] [ProtoMember(1)] public List<int> BufferSizes;

        [DataMember] [ProtoMember(3)] public int MilkRunCycleLength;

        [DataMember] [ProtoMember(2)] public List<int> OrderUpToLevels;

        public override string ToString() {
            var bsizes = string.Join(",", BufferSizes);
            var levels = string.Join(",", OrderUpToLevels);
            return $"{nameof(BufferSizes)}: {bsizes}, {nameof(MilkRunCycleLength)}: {MilkRunCycleLength}, {nameof(OrderUpToLevels)}: {levels}";
        }
    }
}