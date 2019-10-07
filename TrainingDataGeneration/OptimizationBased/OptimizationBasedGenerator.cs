using System;
using System.Collections.Generic;
using System.Linq;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization;
using MilkrunOptimizer.Optimization.LocalSolver;

namespace MilkrunOptimizer.TrainingDataGeneration.OptimizationBased {
    public class OptimizationBasedGenerator {
        public static List<Sample> GenerateSamplesForAllocationProblem(MilkrunBufferAllocationProblem problem, int targetSampleCount = 1000) {
            Console.WriteLine($"Generating training samples for problem={problem} with target sample count of {targetSampleCount}...");
            var evaluator = new SimAndRememberEvaluator(problem);
            LocalSolverOptimizer.Solve(problem, evaluator, targetSampleCount);
            return evaluator.Samples;
        }
        
        public static TrainingData BatchGenerateTrainingData(int numProblems, int numSamplesPerProblem) {
            var sampleLists = Enumerable.Range(1, numProblems).Select(seed => GenerateSamplesForAllocationProblem(ProblemInstanceGenerator.Generate(seed), numSamplesPerProblem));
            return new TrainingData { Samples = sampleLists.SelectMany(i => i).ToList() };
        }
    }
}