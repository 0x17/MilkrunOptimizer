using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class MilkrunBufferAllocationProblem
    {
        [DataMember] [ProtoMember(2)] public List<float> BufferCostFactors;

        [DataMember] [ProtoMember(4)] public float MilkRunInverseCostFactor;

        [DataMember] [ProtoMember(5)] public float MinProductionRate;

        [DataMember] [ProtoMember(3)] public List<float> OrderUpToCostFactors;

        [DataMember] [ProtoMember(1)] public List<float> ProcessingRates;
    }
}