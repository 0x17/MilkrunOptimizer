using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model
{
    [DataContract]
    [ProtoContract]
    public class TrainValidationData
    {
        [DataMember] [ProtoMember(1)] public TrainingData Training;

        [DataMember] [ProtoMember(2)] public TrainingData Validation;
    }
}