using Keras.Models;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer.Optimization
{
    internal abstract class BaseEvaluator
    {
        protected readonly FlowlineConfiguration flc;

        public BaseEvaluator(MilkrunBufferAllocationProblem problem)
        {
            flc = InstanceGenerator.Generate(1);
            for (var i = 0; i < flc.NumMachines; i++)
                flc.Machines[i].ProcessingRate = problem.ProcessingRates[i];
        }

        protected void ExtractDataFromContext(LSNativeContext context)
        {
            flc.MilkRunCycleLength = (int) context.GetIntValue(0);
            for (var i = 0; i < flc.NumMachines; i++)
                flc.Machines[i].OrderUpToMilkLevel = (int) context.GetIntValue(1 + i);

            for (var i = 0; i < flc.NumBuffers; i++)
                flc.Buffers[i].Size = (int) context.GetIntValue(1 + flc.NumMachines + i);
        }

        public abstract double Evaluate(LSNativeContext context);
    }

    internal class SimulationEvaluator : BaseEvaluator
    {
        public SimulationEvaluator(MilkrunBufferAllocationProblem problem) : base(problem)
        {
        }

        public override double Evaluate(LSNativeContext context)
        {
            ExtractDataFromContext(context);
            return SimulationRunner.ProductionRateForConfiguration(flc);
        }
    }

    internal class NetworkEvaluator : BaseEvaluator
    {
        private readonly ProductionRatePredictor predictor;

        public NetworkEvaluator(MilkrunBufferAllocationProblem problem, BaseModel model) : base(problem)
        {
            predictor = new ProductionRatePredictor(model);
        }

        public override double Evaluate(LSNativeContext context)
        {
            ExtractDataFromContext(context);
            return predictor.PredictConfig(flc);
        }
    }
}