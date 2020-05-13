using System.Collections.Generic;
using System.Linq;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization.LocalSolver.Evaluators;

namespace MilkrunOptimizer.TrainingDataGeneration.OptimizationBased {
    class SimAndRememberEvaluator : SimulationEvaluator {
        public List<Sample> Samples { get; } = new List<Sample>();

        public SimAndRememberEvaluator(MilkrunBufferAllocationProblem problem) : base(problem) {}

        public override double Evaluate(LSExternalArgumentValues context) {
            ExtractDataFromContext(context);
            var newSample = Flc.ToSample();
            var oldSample = Samples.Find(sample => sample.ToFloats().Equals(newSample.ToFloats()));
            if(oldSample == null) {
                var result = SimulationRunner.ProductionRateForConfiguration(Flc);
                newSample.ProductionRate = result;
                Samples.Add(newSample);
                return result;
            }
            return oldSample.ProductionRate;
        }
    }
}