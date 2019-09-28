using System;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Optimization {
    public static class ProblemInstanceGenerator {
        public static MilkrunBufferAllocationProblem Generate(int seed) {
            var rand = new Random(seed);
            const int numMachines = 4;
            const int numBuffers = numMachines - 1;
            var prates = Enumerable.Range(0, numMachines).Select(ix => Utils.Uniform(rand, 0.8f, 1.2f)).ToList();
            return new MilkrunBufferAllocationProblem {
                ProcessingRates = prates,
                MinProductionRate = 0.8f * prates.Min(),
                BufferCostFactors = Enumerable.Range(0, numBuffers).Select(ix => Utils.Uniform(rand, 10.0f, 100.0f))
                    .ToList(),
                OrderUpToCostFactors = Enumerable.Range(0, numMachines).Select(ix => Utils.Uniform(rand, 10.0f, 100.0f))
                    .ToList(),
                MilkRunInverseCostFactor = Utils.Uniform(rand, 0.0f, 1.0f)
            };
        }
    }
}