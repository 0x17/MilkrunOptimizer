using System;
using System.Linq;
using System.Threading;
using Keras.Models;
using MilkrunOptimizer.Model;
using Numpy;
using Python.Runtime;

namespace MilkrunOptimizer.NeuralNetwork {
    public class ProductionRatePredictor {
        private readonly BaseModel _model;

        public ProductionRatePredictor(BaseModel model) {
            _model = model;
        }

        public static Sample ConfigToSample(FlowlineConfiguration config) {
            return new Sample {
                BufferSizes = config.Buffers.Select(buf => buf.Size).ToList(),
                MaterialRatios = config.Machines
                    .Select(machine => (float) machine.OrderUpToMilkLevel / (float) config.MilkRunCycleLength).ToList(),
                ProcessingRates = config.Machines.Select(machine => machine.ProcessingRate).ToList()
            };
        }

        public float PredictConfig(FlowlineConfiguration config) {
            return Predict(ConfigToSample(config));
        }
        
        public float Predict(Sample sample) {
            float pr;
            using(Py.GIL()) {
                var xs = NetworkTrainer.XsFromSample(sample);
                var arr = _model.Predict(xs, verbose:0);
                pr = (float) arr[0, 0];
            }
            return pr;
        }
    }
}