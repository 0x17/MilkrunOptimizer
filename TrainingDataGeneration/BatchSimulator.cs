using System.Linq;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.TrainingDataGeneration
{
    public static class BatchSimulator
    {
        public static Sample SingleLine(int seed)
        {
            var flc = InstanceGenerator.Generate(seed);
            var rate = SimulationRunner.ProductionRateForConfiguration(flc);
            return SampleForLineWithRate(flc, rate);
        }

        public static TrainingData LinesFromSeedRange(int seedLbIncl, int seedUbIncl)
        {
            return new TrainingData
            {
                Samples = Enumerable.Range(seedLbIncl, seedUbIncl - seedLbIncl + 1).Select(SingleLine).ToList()
            };
        }

        private static Sample SampleForLineWithRate(FlowlineConfiguration flc, float rate)
        {
            var sample = new Sample
            {
                BufferSizes = flc.Buffers.Select(buf => buf.Size).ToList(),
                ProcessingRates = flc.Machines.Select(machine => machine.ProcessingRate).ToList(),
                MaterialRatios = flc.Machines
                    .Select(machine => (float) machine.OrderUpToMilkLevel / (float) flc.MilkRunCycleLength).ToList(),
                ProductionRate = rate
            };
            return sample;
        }
    }
}