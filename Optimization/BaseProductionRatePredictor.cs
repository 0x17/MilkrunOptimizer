using System.Linq;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Optimization {
    public abstract class BaseProductionRatePredictor {
        public float PredictConfig(FlowlineConfiguration config) {
            return Predict(ConfigToSample(config));
        }

        public abstract float Predict(Sample sample);

        private Sample ConfigToSample(FlowlineConfiguration config) {
            return new Sample {
                BufferSizes = config.Buffers.Select(buf => buf.Size).ToList(),
                MaterialRatios = config.Machines
                    .Select(machine => (float) machine.OrderUpToMilkLevel / (float) config.MilkRunCycleLength).ToList(),
                ProcessingRates = config.Machines.Select(machine => machine.ProcessingRate).ToList()
            };
        }
    }
}