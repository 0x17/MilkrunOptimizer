using Keras.Models;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization.LocalSolver.Evaluators {
    internal class PredictorBasedEvaluator : BaseEvaluator {
        private readonly BaseProductionRatePredictor _predictor;

        public PredictorBasedEvaluator(MilkrunBufferAllocationProblem problem, BaseProductionRatePredictor predictor) :
            base(problem) {
            _predictor = predictor;
        }

        public PredictorBasedEvaluator(MilkrunBufferAllocationProblem problem, BaseModel model) : base(problem) {
            _predictor = new KerasNeuralProductionRatePredictor(model);
        }

        public override double Evaluate(LSExternalArgumentValues context) {
            ExtractDataFromContext(context);
            return _predictor.PredictConfig(Flc);
        }
    }
}