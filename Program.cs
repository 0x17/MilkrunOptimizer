using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using MilkrunOptimizer.Cluster;
using MilkrunOptimizer.NeuralNetwork;
using MilkrunOptimizer.Persistence;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer
{
    internal static class Program
    {
        private static void TrainNetwork()
        {
            var td = TrainingDataPersistence.LoadFromDisk("5klines.bin");
            var tvd = NetworkTrainer.Split(td, 0.9f, true);
            var model = NetworkTrainer.TrainNetworkWithData(tvd.Training, tvd.Validation);
            model.Save("trained_model.hdf5");

            var predictor = new ProductionRatePredictor(model);
            var rate = predictor.PredictConfig(InstanceGenerator.Generate(59595959));
            Console.WriteLine("Predicted rate is {0}", rate);
        }

        private static void GenerateTrainingData(int from, int to)
        {
            var td = BatchSimulator.LinesFromSeedRange(from, to);
            var outFilename = $"results_from_{from}_to_{to}.bin";
            TrainingDataPersistence.SaveToDisk(td, outFilename);
        }

        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            //ParseArgs(args);

            
            var model = NetworkTrainer.LoadFromDisk("model.hdf5");
            var predictor = new ProductionRatePredictor(model);
            var rate = predictor.PredictConfig(InstanceGenerator.Generate(59595959));
            Console.WriteLine("Predicted rate is {0}", rate);
            /*var problem = ProblemInstanceGenerator.Generate(1);
            var solution = LocalSolverOptimizer.Solve(problem);
            Console.WriteLine("Solution is {0}", solution);*/
        }

        private static void ParseArgs(string[] args)
        {
            if (args.Contains("BatchSimulation"))
            {
                int fromSeed = 1, toSeed = 10;
                foreach (var arg in args)
                {
                    if (arg.StartsWith("From="))
                        fromSeed = int.Parse(arg.Split("=")[1]);
                    if (arg.StartsWith("To="))
                        toSeed = int.Parse(arg.Split("=")[1]);
                }

                GenerateTrainingData(fromSeed, toSeed);
            }
            else if (args.Contains("TrainNetwork"))
            {
                string fn = args.First(arg => arg.StartsWith("Filename=")).Split("=")[1];
                var td = TrainingDataPersistence.LoadFromDisk(fn);
                var tvd = NetworkTrainer.Split(td, 0.9f, true);
                var model = NetworkTrainer.TrainNetworkWithData(tvd.Training, tvd.Validation);
                model.Save("model.hdf5");
            }
            else if (args.Contains("JobGeneration"))
            {
                JobGenerator.GenerateJobs();
            }
            else if (args.Contains("MergeResults"))
            {
                var mergedData = MergeResultFiles.MergeDataInPath(".", ".bin", showProgress: true);
                TrainingDataPersistence.SaveToDisk(mergedData, "merged.bin");
            }
            else if (args.Contains("PrintData"))
            {
                string fn = args.First(arg => arg.StartsWith("Filename=")).Split("=")[1];
                int maxCount = Int32.MaxValue;
                if(args.Any(arg => arg.StartsWith("NumRows=")))
                    maxCount = int.Parse(args.First(arg => arg.StartsWith("NumRows=")).Split("=")[1]);
                var samples = TrainingDataPersistence.LoadFromDisk(fn).Samples;
                Console.WriteLine(
                    $"Overall number of samples is {samples.Count}. Now showing up to {maxCount} samples...");
                int ctr = 0;
                foreach(var sample in samples)
                {
                    Console.WriteLine(sample);
                    if (ctr++ >= maxCount)
                        break;
                }
            }
            else
            {
                Console.WriteLine("Nothing to do... please use action BatchSimulation, MergeResults or JobGeneration");
                Console.WriteLine("Usage 1: dotnet MilkrunOptimizer.dll PrintData Filename=test.bin NumRows=10");
            }
        }
    }
}