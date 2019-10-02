using System;
using System.Collections.Generic;
using System.Globalization;
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
using MilkrunOptimizer.Optimization.SimulatedAnnealing;
using MilkrunOptimizer.Persistence;
using MilkrunOptimizer.TrainingDataGeneration;

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

            void TrainNetwork() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.5f, true);
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
                        sol = LocalSolverOptimizer.Solve(problem, predictor);
                        break;
                }

                Console.WriteLine("Solution of optimization = {0}", sol);
                Console.WriteLine("Production rate from predictor = {0}", predictor.Predict(sol.ToSample(problem.ProcessingRates)));
                Console.WriteLine("Production rate from simulation = {0}", SimulationRunner.ProductionRateForConfiguration(sol.ToFlowlineConfiguration(problem.ProcessingRates)));
                Console.WriteLine("Minimum production rate = {0}", problem.MinProductionRate);
            }

            void TrainForest() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.5f, true);
                //var model = NetworkTrainer.TrainNetworkWithData(tvd.Training, tvd.Validation);
                //model.Save("model.hdf5");
                MLContext context = new MLContext(23);
                var transformer = ModelTrainer.TrainModelWithData(context, tvd.Training, tvd.Validation, out var schema);
                context.Model.Save(transformer, schema,"model.zip");
            }

            void AutoMl() {
                var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                var tvd = MlUtils.Split(td, 0.5f, true);
                MLContext context = new MLContext(23);
                ModelSearch.AutoMlOnDataset(context, tvd.Training, tvd.Validation);
            }

            var availableActions = new List<Action> {
                BatchSimulation,
                TrainNetwork,
                JobGeneration,
                MergeResults,
                PrintData,
                Optimize,
                TrainForest,
                AutoMl
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