using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class Sample
    {
        [DataMember] [ProtoMember(3)] public List<int> BufferSizes;

        [DataMember] [ProtoMember(2)] public List<float> MaterialRatios;

        [DataMember] [ProtoMember(1)] public List<float> ProcessingRates;

        [DataMember] [ProtoMember(4)] public float ProductionRate;
    }
}