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
            string  outFilename = $"results_from_{from}_to_{to}.bin";
            TrainingDataPersistence.SaveToDisk(td, outFilename);
        }
        
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (args.Contains("BatchSimulation"))
            {
                int fromSeed = 1, toSeed = 10;
                foreach (string arg in args)
                {
                    if (arg.StartsWith("From="))
                        fromSeed = int.Parse(arg.Split("=")[1]);
                    if (arg.StartsWith("To="))
                        toSeed = int.Parse(arg.Split("=")[1]);
                }

                GenerateTrainingData(fromSeed, toSeed);
            } else if (args.Contains("JobGeneration"))
            {
                JobGenerator.GenerateJobs();
            }
            else
            {
                Console.WriteLine("Nothing to do... please use action BatchSimulation or JobGeneration");
            }

            /*foreach (var sample in td.Samples)
            {
                Console.WriteLine("Sample = {0}", sample);
            }*/

            /*var td = TrainingDataPersistence.LoadFromDisk("simresults.bin");
            var model = NetworkTrainer.TrainNetworkWithData(td);
            //var model = NetworkTrainer.LoadFromDisk("trained_model.hdf5");*/

            /*var problem = ProblemInstanceGenerator.Generate(1);
            var solution = LocalSolverOptimizer.Solve(problem);
            Console.WriteLine("Solution is {0}", solution);*/
        }
    }
}