using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class Machine {
        [DataMember] [ProtoMember(5)] public float CoefficientVariationSquared;

        [DataMember] [ProtoMember(2)] public float FailRate;

        [DataMember] [ProtoMember(1)] public int OperationalUnits;

        [DataMember] [ProtoMember(6)] public int OrderUpToMilkLevel;

        [DataMember] [ProtoMember(4)] public float ProcessingRate;

        [DataMember] [ProtoMember(3)] public float ReplacementArrivalRate;
        
        public static Machine ConstructDefaultMachine(float processingRate, int orderUpLevel) {
            return new Machine {
                CoefficientVariationSquared = 1.0f,
                OperationalUnits = 1,
                FailRate = 0.01f,
                ReplacementArrivalRate = -0.1f,
                ProcessingRate = processingRate,
                OrderUpToMilkLevel = orderUpLevel
            };
        }
    }
}