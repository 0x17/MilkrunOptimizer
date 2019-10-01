using Keras.Models;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization;
using Python.Runtime;

namespace MilkrunOptimizer.NeuralNetwork {
    public class KerasNeuralProductionRatePredictor : BaseProductionRatePredictor {
        private readonly BaseModel _model;

        public KerasNeuralProductionRatePredictor(BaseModel model) {
            _model = model;
        }
        
        public override float Predict(Sample sample) {
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