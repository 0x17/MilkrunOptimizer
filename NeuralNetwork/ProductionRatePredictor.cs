using System.Linq;
using Keras.Models;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.NeuralNetwork
{
    public class ProductionRatePredictor
    {
        private readonly BaseModel model;

        public ProductionRatePredictor(BaseModel model)
        {
            this.model = model;
        }

        public static Sample ConfigToSample(FlowlineConfiguration config)
        {
            return new Sample
            {
                BufferSizes = config.Buffers.Select(buf => buf.Size).ToList(),
                MaterialRatios = config.Machines
                    .Select(machine => (float) machine.OrderUpToMilkLevel / (float) config.MilkRunCycleLength).ToList(),
                ProcessingRates = config.Machines.Select(machine => machine.ProcessingRate).ToList()
            };
        }

        public float PredictConfig(FlowlineConfiguration config)
        {
            return Predict(ConfigToSample(config));
        }

        public float Predict(Sample sample)
        {
            var arr = model.Predict(NetworkTrainer.XsFromSample(sample));
            return (float) arr[0, 0];
        }
    }
}