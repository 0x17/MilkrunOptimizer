using System.Collections.Generic;
using System.Linq;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization
{
    internal class Solution
    {
        private readonly List<int> _bufferSizes;
        private readonly int _milkRunCycleLength;
        private readonly List<int> _orderUpToLevels;

        public Solution(int milkRunCycleLength, List<int> orderUpToLevels, List<int> bufferSizes)
        {
            _milkRunCycleLength = milkRunCycleLength;
            _orderUpToLevels = orderUpToLevels;
            _bufferSizes = bufferSizes;
        }

        public float ProductionRate(MilkrunBufferAllocationProblem problem, ProductionRatePredictor predictor)
        {
            var s = new Sample
            {
                BufferSizes = _bufferSizes,
                MaterialRatios = _orderUpToLevels.Select(oul => (float) oul / (float) _milkRunCycleLength).ToList(),
                ProcessingRates = problem.ProcessingRates
            };
            return predictor.Predict(s);
        }
    }

    public class SimulatedAnnealingOptimizer
    {
        public static MilkrunBufferAllocationSolution Solve(MilkrunBufferAllocationProblem problem)
        {
            return null;
        }
    }
}