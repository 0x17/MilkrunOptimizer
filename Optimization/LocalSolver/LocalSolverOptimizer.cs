﻿using System;
using System.Linq;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization.LocalSolver.Evaluators;
using Python.Runtime;

namespace MilkrunOptimizer.Optimization.LocalSolver {
    public static class LocalSolverOptimizer {
        public static MilkrunBufferAllocationSolution Solve(MilkrunBufferAllocationProblem problem,
            BaseEvaluator evaluator,
            long iterationLimit = 1000,
            bool kerasBased = false) {
            if (evaluator != null && kerasBased)
                PythonEngine.BeginAllowThreads();

            using var ls = new localsolver.LocalSolver();
            var model = ls.GetModel();

            var milkrunCycleLength = model.Int(1, 120);
            var orderUpToLevels = Enumerable.Range(0, problem.ProcessingRates.Count).Select(i => model.Int(1, 400))
                .ToList();
            var bufferSizes = Enumerable.Range(0, problem.BufferCostFactors.Count).Select(i => model.Int(0, 500))
                .ToList();
            
            void SetupBoundsForMaterialRatio() {
                for(int i=0; i<orderUpToLevels.Count; i++) {
                    model.AddConstraint(milkrunCycleLength * problem.ProcessingRates[i] >= orderUpToLevels[i]);
                    model.AddConstraint(milkrunCycleLength * problem.MinProductionRate <= orderUpToLevels[i]);
                }
            }
            
            void SetupMinimumProductionRateConstraint() {
                var func = model.CreateDoubleExternalFunction(evaluator.Evaluate);
                var rateEvalCall = model.Call(func);
                rateEvalCall.AddOperand(milkrunCycleLength);
                rateEvalCall.AddOperands(orderUpToLevels);
                rateEvalCall.AddOperands(bufferSizes);
                const float epsilon = 0.005f;
                model.Constraint(rateEvalCall >= problem.MinProductionRate + epsilon);
            }

            void SetupObjectiveFunction() {
                var milkrunCycleCosts = 1.0f / milkrunCycleLength * problem.MilkRunInverseCostFactor;
                var bufferCosts = model.Sum(bufferSizes.Select((bsize, ix) => bsize * problem.BufferCostFactors[ix]));
                var orderUpToCosts =
                    model.Sum(orderUpToLevels.Select((level, ix) => level * problem.OrderUpToCostFactors[ix]));
                model.Minimize(milkrunCycleCosts + bufferCosts + orderUpToCosts);
            }
            
            void SetInitialSolution() {
                bufferSizes.ForEach(bufferSize => bufferSize.SetValue(500));
                orderUpToLevels.ForEach(oul => oul.SetValue((int)Math.Ceiling(problem.MinProductionRate*100.0f)));
                milkrunCycleLength.SetValue(100);
            }

            SetupBoundsForMaterialRatio();
            SetupMinimumProductionRateConstraint();
            SetupObjectiveFunction();
            model.Close();
            
            SetInitialSolution();

            //ls.GetParam().SetTimeLimit(10);
            ls.GetParam().SetIterationLimit(iterationLimit);
            ls.GetParam().SetNbThreads(1);
            ls.Solve();
            
            return new MilkrunBufferAllocationSolution {
                BufferSizes = bufferSizes.Select(bufferSize => (int) bufferSize.GetIntValue()).ToList(),
                MilkRunCycleLength = (int) milkrunCycleLength.GetIntValue(),
                OrderUpToLevels = orderUpToLevels.Select(oul => (int) oul.GetIntValue()).ToList()
            };
        }
    }
}