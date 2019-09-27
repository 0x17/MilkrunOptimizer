using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class MilkrunBufferAllocationSolution
    {
        [DataMember] [ProtoMember(1)] public List<int> BufferSizes;

        [DataMember] [ProtoMember(3)] public int MilkRunCycleLength;

        [DataMember] [ProtoMember(2)] public List<int> OrderUpToLevels;
    }
}