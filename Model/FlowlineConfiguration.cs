using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class FlowlineConfiguration
    {
        [DataMember] [ProtoMember(6)] public List<Buffer> Buffers;

        [DataMember] [ProtoMember(5)] public List<Machine> Machines;

        [DataMember] [ProtoMember(2)] public int MilkRunCycleLength;

        [DataMember] [ProtoMember(4)] public int NumBuffers;

        [DataMember] [ProtoMember(3)] public int NumMachines;

        [DataMember] [ProtoMember(1)] public float RequiredRelativeMarginOfError;
    }
}