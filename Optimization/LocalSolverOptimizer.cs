using System.Linq;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization {
    public static class LocalSolverOptimizer {
        public static MilkrunBufferAllocationSolution Solve(MilkrunBufferAllocationProblem problem) {
            var numMachines = problem.ProcessingRates.Count;
            var numBuffers = problem.BufferCostFactors.Count;
            var solution = new MilkrunBufferAllocationSolution();
            using (var ls = new LocalSolver()) {
                var model = ls.GetModel();

                var milkrunCycleLength = model.Int(1, 120);
                var orderUpToLevels = Enumerable.Range(0, numMachines).Select(i => model.Int(1, 400)).ToList();
                var bufferSizes = Enumerable.Range(0, numBuffers).Select(i => model.Int(0, 500)).ToList();

                var dnn = NetworkTrainer.LoadFromDisk("trained_model.hdf5");

                var evaluator = new NetworkEvaluator(problem, dnn);
                var func = model.CreateNativeFunction(evaluator.Evaluate);
                var rateEvalCall = model.Call(func);
                rateEvalCall.AddOperand(milkrunCycleLength);
                rateEvalCall.AddOperands(orderUpToLevels);
                rateEvalCall.AddOperands(bufferSizes);

                model.Constraint(rateEvalCall >= problem.MinProductionRate);

                var milkrunCycleCosts = 1.0f / milkrunCycleLength * problem.MilkRunInverseCostFactor;
                var bufferCosts = model.Sum(bufferSizes.Select((bsize, ix) => bsize * problem.BufferCostFactors[ix]));
                var orderUpToCosts =
                    model.Sum(orderUpToLevels.Select((level, ix) => level * problem.OrderUpToCostFactors[ix]));

                model.Minimize(milkrunCycleCosts + bufferCosts + orderUpToCosts);

                ls.GetParam().SetTimeLimit(10);
                ls.Solve();
            }

            return solution;
        }
    }
}