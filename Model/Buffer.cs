using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class Buffer {
        [DataMember] [ProtoMember(4)] public int DownMachine;

        [DataMember] [ProtoMember(1)] public int NumMachinesSurrounding;

        [DataMember] [ProtoMember(6)] public float SelectionProbability;

        [DataMember] [ProtoMember(5)] public int Size;

        [DataMember] [ProtoMember(3)] public int Up2Machine;

        [DataMember] [ProtoMember(2)] public int UpMachine;
        
        public static Buffer ConstructDefaultBuffer(int size, int index) {
            return new Buffer {
                NumMachinesSurrounding = 2,
                UpMachine = index+1,
                Up2Machine = 0,
                DownMachine = index+2,
                Size = size,
                SelectionProbability = 1.0f
            };
        }
    }
}