using System;
using System.Diagnostics;
using System.Linq;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.TrainingDataGeneration
{
    public static class BatchSimulator
    {
        public static Sample SingleLine(int seed, float? progress = null, Stopwatch sw = null)
        {
            if (progress != null)
            {
                var etaStr = "";
                if (sw != null)
                {
                    var estimatedTotalTime =
                        (long) Math.Round((double) (sw.ElapsedMilliseconds * (1.0f / progress) - 1.0f));
                    var eta = estimatedTotalTime - sw.ElapsedMilliseconds;
                    etaStr = $"ETA {eta / 1000.0f / 3600.0f} hours or {eta / 1000.0f / 60.0f} minutes";
                }

                Console.Write("\rSimulation progress {0}... {1}", progress, etaStr);
                if (progress >= 1.0f)
                    Console.WriteLine();
            }

            var flc = InstanceGenerator.Generate(seed);
            var rate = SimulationRunner.ProductionRateForConfiguration(flc);
            return SampleForLineWithRate(flc, rate);
        }

        public static TrainingData LinesFromSeedRange(int seedLbIncl, int seedUbIncl)
        {
            var sw = new Stopwatch();
            sw.Start();
            var td = new TrainingData
            {
                Samples = Enumerable.Range(seedLbIncl, seedUbIncl - seedLbIncl + 1).Select((seed, ix) =>
                    SingleLine(seed, (seed - seedLbIncl + 1) / (float) (seedUbIncl - seedLbIncl + 1), sw)).ToList()
            };
            sw.Stop();
            return td;
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