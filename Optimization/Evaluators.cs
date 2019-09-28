using Keras.Models;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;
using MilkrunOptimizer.TrainingDataGeneration;

namespace MilkrunOptimizer.Optimization
{
    internal abstract class BaseEvaluator
    {
        protected readonly FlowlineConfiguration Flc;

        public BaseEvaluator(MilkrunBufferAllocationProblem problem)
        {
            Flc = InstanceGenerator.Generate(1);
            for (var i = 0; i < Flc.NumMachines; i++)
                Flc.Machines[i].ProcessingRate = problem.ProcessingRates[i];
        }

        protected void ExtractDataFromContext(LSNativeContext context)
        {
            Flc.MilkRunCycleLength = (int) context.GetIntValue(0);
            for (var i = 0; i < Flc.NumMachines; i++)
                Flc.Machines[i].OrderUpToMilkLevel = (int) context.GetIntValue(1 + i);

            for (var i = 0; i < Flc.NumBuffers; i++)
                Flc.Buffers[i].Size = (int) context.GetIntValue(1 + Flc.NumMachines + i);
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
            return SimulationRunner.ProductionRateForConfiguration(Flc);
        }
    }

    internal class NetworkEvaluator : BaseEvaluator
    {
        private readonly ProductionRatePredictor _predictor;

        public NetworkEvaluator(MilkrunBufferAllocationProblem problem, BaseModel model) : base(problem)
        {
            _predictor = new ProductionRatePredictor(model);
        }

        public override double Evaluate(LSNativeContext context)
        {
            ExtractDataFromContext(context);
            return _predictor.PredictConfig(Flc);
        }
    }
}