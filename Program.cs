﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.ML;
using MilkrunOptimizer.ClassicML;
using MilkrunOptimizer.Cluster;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;
using MilkrunOptimizer.Optimization;
using MilkrunOptimizer.Optimization.LocalSolver;
using MilkrunOptimizer.Optimization.LocalSolver.Evaluators;
using MilkrunOptimizer.Optimization.SimulatedAnnealing;
using MilkrunOptimizer.Persistence;
using MilkrunOptimizer.TrainingDataGeneration;
using MilkrunOptimizer.TrainingDataGeneration.Exhaustive;
using MilkrunOptimizer.TrainingDataGeneration.OptimizationBased;
using ServiceStack;
using ServiceStack.Text;

namespace MilkrunOptimizer {
    internal static class Program {
        private static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            ParseArgs(args);
        }

        private static void ParseArgs(string[] args) {
            var structuredArgs = StructuredArguments.FromStrings(args);

            void BatchSimulation() {
                var fromSeed = structuredArgs.AsIntOrDefault("From", 1);
                var toSeed = structuredArgs.AsIntOrDefault("To", 10);
                var td = BatchSimulator.LinesFromSeedRange(fromSeed, toSeed);
                var outFilename = $"results_from_{fromSeed}_to_{toSeed}.bin";
                TrainingDataPersistence.SaveToDisk(td, outFilename);
            }
            
            void BatchSimulationOptimizationBased() {
                var td = OptimizationBasedGenerator.BatchGenerateTrainingData(100, 100);
                TrainingDataPersistence.SaveToDisk(td, $"new_results.bin");
            }

            void TrainNetwork() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.5f, false);
                var model = NetworkTrainer.TrainNetworkWithData(tvd.Training, tvd.Validation);
                model.Save("model.hdf5");
            }

            void JobGeneration() {
                JobGenerator.GenerateJobs();
            }

            void MergeResults() {
                var mergedData = MergeResultFiles.MergeDataInPath(".", ".bin");
                TrainingDataPersistence.SaveToDisk(mergedData, "merged.bin");
            }

            void PrintData() {
                var maxCount = structuredArgs.AsIntOrDefault("NumRows", int.MaxValue);
                var samples = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename")).Samples;
                Console.WriteLine(
                    $"Overall number of samples is {samples.Count}. Now showing up to {maxCount} samples...");
                var ctr = 0;
                foreach (var sample in samples) {
                    Console.WriteLine(sample);
                    if (ctr++ >= maxCount)
                        break;
                }
            }

            void Optimize() {
                var methodName = structuredArgs.AsStringOrDefault("Method", "LocalSolver");
                var problem = ProblemInstanceGenerator.Generate(23);
                BaseProductionRatePredictor predictor = null;
                //predictor = new KerasNeuralProductionRatePredictor(ModelPersistence.LoadFromDisk("model.hdf5"));
                //predictor = new OnnxNeuralProductionRatePredictor("converted.onnx");
                predictor = new MlProductionRatePredictor("model.zip");
                MilkrunBufferAllocationSolution sol = null;
                switch (methodName) {
                    case "SimulatedAnnealing":
                        sol = SimAnnealOptimizer.Solve(problem, predictor, 1000, 1.0f);
                        break;
                    case "LocalSolver":
                        //var evaluator = new SimulationEvaluator(problem);
                        var evaluator = new PredictorBasedEvaluator(problem, predictor);
                        sol = LocalSolverOptimizer.Solve(problem, evaluator);
                        break;
                }

                Console.WriteLine("Solution of optimization = {0}", sol);
                Console.WriteLine("Production rate from predictor = {0}", predictor.Predict(sol.ToSample(problem.ProcessingRates)));
                Console.WriteLine("Production rate from simulation = {0}", SimulationRunner.ProductionRateForConfiguration(sol.ToFlowlineConfiguration(problem.ProcessingRates)));
                Console.WriteLine("Minimum production rate = {0}", problem.MinProductionRate);
            }

            void TrainForest() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.999f, true);
                MLContext context = new MLContext(23);
                var transformer = ModelTrainer.TrainModelWithData(context, tvd.Training, tvd.Validation, out var schema);
                context.Model.Save(transformer, schema,"model.zip");
            }

            void AutoMl() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 1.0f, true);
                MLContext context = new MLContext(23);
                ModelSearch.AutoMlOnDataset(context, tvd.Training, tvd.Validation);
            }
            
            void DumpPredictionErrors() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.5f, false);
                var dnn = new OnnxNeuralProductionRatePredictor("converted.onnx");
                
                PredictionSample Predict(Sample sample) {
                    var predictedRate = dnn.Predict(sample);
                    float deviation = predictedRate - sample.ProductionRate;
                    return new PredictionSample(sample, deviation);
                }
                
                var psamples = tvd.Validation.Samples.Take(100).Select(Predict).ToList();
                File.WriteAllText("deviations.csv", CsvSerializer.SerializeToCsv(psamples));
            }

            void TestExhaustiveGenerator() {
                int numMachines = 4;
                int numBuffers = numMachines - 1;
                var features = new List<FeatureDescription> {
                    new FeatureDescription() {
                        IsDiscrete = true,
                        LowerBound = 0,
                        UpperBound = 120,
                        Name="milkrun_cycle_length"
                    }
                };
                features.AddRange(Enumerable.Range(0, numMachines).Select(i=>
                    new FeatureDescription {
                        IsDiscrete = false,
                        LowerBound = 0.8,
                        UpperBound = 1.2,
                        Name=$"processing_rate_{i+1}"
                    }));
                features.AddRange(Enumerable.Range(0, numMachines).Select(i=>
                    new FeatureDescription {
                        IsDiscrete = true,
                        LowerBound = 0,
                        UpperBound = 10,
                        Name=$"order_up_to_level_{i+1}"
                    }));
                features.AddRange(Enumerable.Range(0, numBuffers).Select(i=>
                    new FeatureDescription {
                        IsDiscrete = true,
                        LowerBound = 1,
                        UpperBound = 80,
                        Name=$"buffer_size_{i+1}"
                    }));
                var samples = OrthoLatinHyperCube.PickSamples(features.ToArray(), 80, 1);
                var lines = new List<string> {
                    string.Join(";", samples.First().ColumnNames())
                };
                lines.AddRange(samples.Select(sample => string.Join(";", sample.ToFloats())));
                File.WriteAllText("ortholatinhypercube.csv", string.Join("\n", lines));
            }

            var availableActions = new List<Action> {
                BatchSimulation,
                TrainNetwork,
                JobGeneration,
                MergeResults,
                PrintData,
                Optimize,
                TrainForest,
                AutoMl,
                BatchSimulationOptimizationBased,
                DumpPredictionErrors,
                TestExhaustiveGenerator
            };

            var actionMappings =
                availableActions.ToDictionary(action => Utils.NameOfLocalActionFunction("ParseArgs", action),
                    action => action);
            if (args.Length >= 1) {
                var action = structuredArgs.GetAction();
                if (actionMappings.ContainsKey(action)) {
                    actionMappings[action]();
                    return;
                }
            }

            ShowUsage(actionMappings);
        }

        private static void ShowUsage(Dictionary<string, Action> actionMappings) {
            Console.WriteLine("Arguments do not match any known operation");
            Console.WriteLine("Usage 1: dotnet MilkrunOptimizer.dll PrintData Filename=test.bin NumRows=10");
            Console.WriteLine("Known actions are: " + string.Join(", ", actionMappings.Keys));
        }
    }
}