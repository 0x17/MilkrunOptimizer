using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer.Optimization.LocalSolver.Evaluators {
    internal class SimulationEvaluator : BaseEvaluator {
        public SimulationEvaluator(MilkrunBufferAllocationProblem problem) : base(problem) {
        }

        public override double Evaluate(LSExternalArgumentValues context) {
            ExtractDataFromContext(context);
            return SimulationRunner.ProductionRateForConfiguration(Flc);
        }
    }
}