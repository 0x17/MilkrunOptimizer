using System.Collections.Generic;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class TrainingData {
        [DataMember] [ProtoMember(1)] public List<Sample> Samples;
    }
}