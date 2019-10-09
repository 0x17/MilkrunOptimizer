using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ProtoBuf;

namespace MilkrunOptimizer.Model {
    [DataContract]
    [ProtoContract]
    public class MilkrunBufferAllocationSolution {
        [DataMember] [ProtoMember(1)] public List<int> BufferSizes;

        [DataMember] [ProtoMember(3)] public int MilkRunCycleLength;

        [DataMember] [ProtoMember(2)] public List<int> OrderUpToLevels;

        public override string ToString() {
            var bsizes = string.Join(",", BufferSizes);
            var levels = string.Join(",", OrderUpToLevels);
            return
                $"{nameof(BufferSizes)}: {bsizes}, {nameof(MilkRunCycleLength)}: {MilkRunCycleLength}, {nameof(OrderUpToLevels)}: {levels}";
        }

        public FlowlineConfiguration ToFlowlineConfiguration(List<float> processingRates) {
            return new FlowlineConfiguration {
                Buffers = BufferSizes.Select(Buffer.ConstructDefaultBuffer).ToList(),
                Machines = OrderUpToLevels.Select((oul,i) => Machine.ConstructDefaultMachine(processingRates[i], oul)).ToList(),
                MilkRunCycleLength = MilkRunCycleLength,
                NumBuffers = BufferSizes.Count,
                NumMachines = OrderUpToLevels.Count,
                RequiredRelativeMarginOfError = 0.005f
            };
        }

        public Sample ToSample(List<float> processingRates) {
            return new Sample {
                BufferSizes = BufferSizes,
                OrderUpToLevels = OrderUpToLevels,
                MilkrunCycleLength = MilkRunCycleLength,
                ProcessingRates = processingRates,
                ProductionRate = 0.0f
            };
        }
    }
}