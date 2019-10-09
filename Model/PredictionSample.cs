using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class PredictionSample : Sample {
        [DataMember] [ProtoMember(5)]
        public float PredictionError;

        public PredictionSample() {
        }

        public PredictionSample(Sample sample, float deviation) {
            this.PredictionError = deviation; 
        }
    }
}