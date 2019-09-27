using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class Buffer
    {
        [DataMember] [ProtoMember(4)] public int DownMachine;

        [DataMember] [ProtoMember(1)] public int NumMachinesSurrounding;

        [DataMember] [ProtoMember(6)] public float SelectionProbability;

        [DataMember] [ProtoMember(5)] public int Size;

        [DataMember] [ProtoMember(3)] public int Up2Machine;

        [DataMember] [ProtoMember(2)] public int UpMachine;
    }
}