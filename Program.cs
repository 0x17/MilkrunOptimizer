using System;
using System.Globalization;
using System.Threading;
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

        private static void GenerateTrainingData()
        {
            var td = BatchSimulator.LinesFromSeedRange(1, 10000);
            TrainingDataPersistence.SaveToDisk(td, "10klines.bin");
        }
        
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            
            GenerateTrainingData();
            

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