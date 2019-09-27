using System;
using System.Globalization;
using System.Threading;
using MilkrunOptimizer.Optimization;

namespace MilkrunOptimizer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            /*var td = TrainingDataPersistence.LoadFromDisk("simresults.bin");
            var model = NetworkTrainer.TrainNetworkWithData(td);
            //var model = NetworkTrainer.LoadFromDisk("trained_model.hdf5");
            var rate = ProductionRatePredictor.PredictConfig(model, InstanceGenerator.Generate(55595));
            Console.WriteLine("Predicted rate is {0}", rate);*/

            var problem = ProblemInstanceGenerator.Generate(1);
            var solution = LocalSolverOptimizer.Solve(problem);
            Console.WriteLine("Solution is {0}", solution);
        }
    }
}