using System;
using System.Collections.Generic;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Optimization.SimulatedAnnealing {
    public class Solution {
        public readonly List<int> BufferSizes;
        public readonly List<int> OrderUpToLevels;
        public int MilkRunCycleLength;

        public Solution(int milkRunCycleLength, List<int> orderUpToLevels, List<int> bufferSizes) {
            MilkRunCycleLength = milkRunCycleLength;
            OrderUpToLevels = orderUpToLevels;
            BufferSizes = bufferSizes;
        }

        private static void GenericMove(List<int> elems, int ix, MoveType moveType, int minval, int stepWidth = 1) {
            switch (moveType) {
                case MoveType.BigDecrease:
                    elems[ix] -= stepWidth * 5;
                    break;
                case MoveType.SmallDecrease:
                    elems[ix] -= stepWidth;
                    break;
                case MoveType.SmallIncrease:
                    elems[ix] += stepWidth;
                    break;
                case MoveType.Swap:
                    var otherIx = Utils.RandInt(0, elems.Count - 1);
                    elems[ix] += stepWidth;
                    elems[otherIx] -= stepWidth;
                    elems[otherIx] = Math.Max(elems[otherIx], minval);
                    break;
            }
        }

        public void Move(int stepWidth = 1) {
            GenericMove(BufferSizes, Utils.RandInt(0, BufferSizes.Count - 1), (MoveType) Utils.RandInt(1, 4), 0,
                stepWidth);
            GenericMove(OrderUpToLevels, Utils.RandInt(0, OrderUpToLevels.Count - 1), (MoveType) Utils.RandInt(1, 4),
                1, stepWidth);
            MilkRunCycleLength += (Utils.RandInt(0, 1) == 0 ? 1 : -1) * stepWidth;
            MilkRunCycleLength = Math.Max(1, MilkRunCycleLength);
        }

        public Solution Copy() {
            return new Solution(MilkRunCycleLength,
                new List<int>(OrderUpToLevels),
                new List<int>(BufferSizes));
        }

        public float Costs(MilkrunBufferAllocationProblem problem) {
            return 1.0f / MilkRunCycleLength * problem.MilkRunInverseCostFactor +
                   BufferSizes.Select((bufferSize, ix) => bufferSize * problem.BufferCostFactors[ix]).Sum() +
                   OrderUpToLevels.Select((oul, ix) => oul * problem.OrderUpToCostFactors[ix]).Sum();
        }

        public float ProductionRate(MilkrunBufferAllocationProblem problem, BaseProductionRatePredictor predictor) {
            var s = new Sample {
                BufferSizes = BufferSizes,
                MaterialRatios = OrderUpToLevels.Select(oul => (float) oul / (float) MilkRunCycleLength).ToList(),
                ProcessingRates = problem.ProcessingRates
            };
            return predictor.Predict(s);
        }

        public override string ToString() {
            return
                $"BufferSizes: {BufferSizes}, MilkRunCycleLength: {MilkRunCycleLength}, OrderUpToLevels: {OrderUpToLevels}";
        }

        public MilkrunBufferAllocationSolution ToMilkrunSolution() {
            return new MilkrunBufferAllocationSolution {
                MilkRunCycleLength = MilkRunCycleLength,
                BufferSizes = BufferSizes,
                OrderUpToLevels = OrderUpToLevels
            };
        }

        private enum MoveType {
            BigDecrease = 1,
            SmallDecrease,
            SmallIncrease,
            Swap
        }
    }
}