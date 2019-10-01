using Keras.Models;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization.LocalSolver.Evaluators {
    internal class NetworkEvaluator : BaseEvaluator {
        private readonly BaseProductionRatePredictor _predictor;
        
        public NetworkEvaluator(MilkrunBufferAllocationProblem problem, BaseProductionRatePredictor predictor) : base(problem) {
            _predictor = predictor;
        }

        public NetworkEvaluator(MilkrunBufferAllocationProblem problem, BaseModel model) : base(problem) {
            _predictor = new KerasNeuralProductionRatePredictor(model);
        }

        public override double Evaluate(LSNativeContext context) {
            ExtractDataFromContext(context);
            return _predictor.PredictConfig(Flc);
        }
    }
}