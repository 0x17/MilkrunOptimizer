using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using Microsoft.ML.OnnxRuntime;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization;

namespace MilkrunOptimizer.NeuralNetwork {
    public class OnnxNeuralProductionRatePredictor : BaseProductionRatePredictor {
        private InferenceSession _session;

        public OnnxNeuralProductionRatePredictor(string path) {
            _session = new InferenceSession(path);
        }

        public override float Predict(Sample sample) {
            var values = sample.ToFloats().ToArray();
            int[] dimensions = {1, values.Length};
            Tensor<float> tvalues = new DenseTensor<float>(values, dimensions);
            var inputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("dense_1_input", tvalues)
            };
            using var results = _session.Run(inputs);
            return results.First().AsTensor<float>().GetValue(0);
        }
    }
}