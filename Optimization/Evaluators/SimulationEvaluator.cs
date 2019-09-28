using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer.Optimization.Evaluators {
    internal class SimulationEvaluator : BaseEvaluator {
        public SimulationEvaluator(MilkrunBufferAllocationProblem problem) : base(problem) {
        }

        public override double Evaluate(LSNativeContext context) {
            ExtractDataFromContext(context);
            return SimulationRunner.ProductionRateForConfiguration(Flc);
        }
    }
}