using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MilkrunOptimizer.Cluster;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.NeuralNetwork;
using MilkrunOptimizer.Persistence;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer {
    internal static class Program {
        
        private static void Main(string[] args) {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            ParseArgs(args);
        }
        
        private static void GenerateTrainingData(int from, int to) {
            var td = BatchSimulator.LinesFromSeedRange(from, to);
            var outFilename = $"results_from_{from}_to_{to}.bin";
            TrainingDataPersistence.SaveToDisk(td, outFilename);
        }

        private static void ParseArgs(string[] args) {
            var structuredArgs = StructuredArguments.FromStrings(args);

            var actionMappings = new Dictionary<string, Action> {
                {
                    "BatchSimulation", () => {
                        int fromSeed = structuredArgs.AsIntOrDefault("From", 1);
                        int toSeed = structuredArgs.AsIntOrDefault("To", 10);
                        GenerateTrainingData(fromSeed, toSeed);
                    }
                }, {
                    "TrainNetwork", () => {
                        var td = TrainingDataPersistence.LoadFromDisk(structuredArgs.AsString("Filename"));
                        var tvd = NetworkTrainer.Split(td, 0.9f, true);
                        var model = NetworkTrainer.TrainNetworkWithData(tvd.Training, tvd.Validation);
                        model.Save("model.hdf5");
                    }
                },
                {"JobGeneration", () => { JobGenerator.GenerateJobs(); }}, {
                    "MergeResults", () => {
                        var mergedData = MergeResultFiles.MergeDataInPath(".", ".bin");
                        TrainingDataPersistence.SaveToDisk(mergedData, "merged.bin");
                    }
                }, {
                    "PrintData", () => {
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
                }
            };

            if (args.Length >= 1) {
                string action = structuredArgs.GetAction();
                if (actionMappings.ContainsKey(action)) {
                    actionMappings[action]();
                    return;
                }
            }

            ShowUsage(actionMappings);
        }

        private static void ShowUsage(Dictionary<string,Action> actionMappings) {
            Console.WriteLine("Arguments do not match any known operation");
            //foreach(var key in actionMappings.Keys)
            Console.WriteLine("Usage 1: dotnet MilkrunOptimizer.dll PrintData Filename=test.bin NumRows=10");
        }
    }
}