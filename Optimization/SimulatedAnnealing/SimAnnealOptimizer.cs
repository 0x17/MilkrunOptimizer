using System;
using System.Diagnostics;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Optimization.SimulatedAnnealing {
    public class SimAnnealOptimizer {
        public static MilkrunBufferAllocationSolution Solve(MilkrunBufferAllocationProblem problem,
            BaseProductionRatePredictor predictor, int numIterations, float temp0) {
            Utils.SetSeed(23);
            var sw = new Stopwatch();
            sw.Start();

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
                Console.WriteLine("Prediction is {0}", solCandidatePr);
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

        private static Solution GenerateStartSolution(int numMachines) {
            return new Solution(1, Enumerable.Repeat(10, numMachines).ToList(),
                Enumerable.Repeat(1000, numMachines - 1).ToList());
        }

        private static bool AcceptanceMetropolis(float curSolObj, float solCandidateObj, float temp) {
            if (solCandidateObj < curSolObj) return true;
            var pAccept = (float) Math.Exp(-Math.Abs(solCandidateObj - curSolObj) / temp);
            return Utils.RandFloat() < pAccept;
        }
    }
}