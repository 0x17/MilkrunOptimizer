using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization {
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

        public float ProductionRate(MilkrunBufferAllocationProblem problem, ProductionRatePredictor predictor) {
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

    public class SimulatedAnnealingOptimizer {
        public static Solution GenerateStartSolution(int numMachines) {
            return new Solution(1, Enumerable.Repeat(10, numMachines).ToList(),
                Enumerable.Repeat(1000, numMachines - 1).ToList());
        }

        public static bool AcceptanceMetropolis(float curSolObj, float solCandidateObj, float temp) {
            if (solCandidateObj < curSolObj) return true;
            var pAccept = (float) Math.Exp(-Math.Abs(solCandidateObj - curSolObj) / temp);
            return Utils.RandFloat() < pAccept;
        }

        public static MilkrunBufferAllocationSolution Solve(MilkrunBufferAllocationProblem problem,
            ProductionRatePredictor predictor, int numIterations, int temp0) {
            Utils.SetSeed(23);
            var sw = new Stopwatch();

            var curSol = GenerateStartSolution(problem.ProcessingRates.Count);
            var curSolObj = curSol.Costs(problem);

            int numAccept = 0, numReject = 0;
            var lastPrint = -1;
            var printFrequency = 4; // print each 4 seconds

            for (var i = 0; i < numIterations; i++) {
                var temp = (float) (temp0 / Math.Log2(i + 2));
                var solCandidate = curSol.Copy();
                var stepWidth = (int) Math.Round(Math.Max(1.0f, 0.2f * temp));
                solCandidate.Move(stepWidth);
                var solCandidatePr = solCandidate.ProductionRate(problem, predictor);
                if (solCandidatePr >= problem.MinProductionRate) {
                    var solCandidateObj = solCandidate.Costs(problem);
                    if (AcceptanceMetropolis(curSolObj, solCandidateObj, temp)) {
                        numAccept++;
                        curSol = solCandidate;
                        curSolObj = solCandidateObj;
                    }
                    else {
                        numReject++;
                    }
                }
                else {
                    numReject++;
                }

                if (lastPrint == -1 || sw.Elapsed.Seconds - lastPrint > printFrequency) {
                    Console.WriteLine(
                        $"[iter={i}] numAccepted={numAccept}, numRejected={numReject}, bestObj={curSolObj}, stepWidth={stepWidth}, curSol={curSol}");
                    lastPrint = sw.Elapsed.Seconds;
                }
            }

            sw.Stop();
            Console.WriteLine($"Required time in milliseconds = {sw.ElapsedMilliseconds}");

            return curSol.ToMilkrunSolution();
        }
    }
}