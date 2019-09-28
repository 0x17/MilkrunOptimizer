using System;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;
using Buffer = MilkrunOptimizer.Model.Buffer;

namespace MilkrunOptimizer.TrainingDataGeneration
{
    public static class InstanceGenerator
    {
        public static FlowlineConfiguration Generate(int seed)
        {
            var rand = new Random(seed);

            var flc = new FlowlineConfiguration
            {
                RequiredRelativeMarginOfError = 0.005f,
                NumMachines = 4,
                NumBuffers = 3,
                // add 1 since Random.Next the ub is non inclusive!
                MilkRunCycleLength = rand.Next(30, 120+1)
            };

            var variationMue = Utils.Uniform(rand, 0.0f, 0.2f);

            Machine GenerateMachine(int i)
            {
                return new Machine
                {
                    OperationalUnits = 1,
                    FailRate = 0.01f,
                    ReplacementArrivalRate = -0.1f,
                    ProcessingRate = Utils.Uniform(rand, 1.0f - variationMue, 1.0f + variationMue),
                    CoefficientVariationSquared = 1,
                    OrderUpToMilkLevel = (int) Math.Round(Utils.Uniform(rand, 0.5f, 1.5f) * flc.MilkRunCycleLength)
                };
            }

            var bufferMean = Utils.Uniform(rand, 0.0f, 40.0f);
            var bufferFactor = (float) rand.NextDouble();

            Buffer GenerateBuffer(int i)
            {
                return new Buffer
                {
                    NumMachinesSurrounding = 2,
                    UpMachine = i + 1,
                    Up2Machine = 0,
                    DownMachine = i + 2,
                    Size = (int) Math.Round(Utils.Uniform(rand, bufferMean * (1 - bufferFactor),
                        bufferMean * (1 + bufferFactor))),
                    SelectionProbability = 1.0f
                };
            }

            flc.Machines = Enumerable.Range(0, flc.NumMachines).Select(GenerateMachine).ToList();
            flc.Buffers = Enumerable.Range(0, flc.NumBuffers).Select(GenerateBuffer).ToList();

            return flc;
        }
    }
}