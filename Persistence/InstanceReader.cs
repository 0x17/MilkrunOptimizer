using System;
using System.Collections.Generic;
using System.IO;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;
using Buffer = MilkrunOptimizer.Model.Buffer;

namespace MilkrunOptimizer.Persistence
{
    public static class InstanceReader
    {
        public static FlowlineConfiguration FromJson(string path)
        {
            return Utils.LoadObjectFromJson<FlowlineConfiguration>(path);
        }

        private static Machine MachineFromParts(string[] parts)
        {
            return new Machine
            {
                OperationalUnits = int.Parse(parts[0]),
                FailRate = Utils.ToFloat(parts[1]),
                ReplacementArrivalRate = Utils.ToFloat(parts[2]),
                ProcessingRate = Utils.ToFloat(parts[3]),
                CoefficientVariationSquared = Utils.ToFloat(parts[4]),
                OrderUpToMilkLevel = int.Parse(parts[5])
            };
        }

        private static Buffer BufferFromParts(string[] parts)
        {
            return new Buffer
            {
                NumMachinesSurrounding = int.Parse(parts[0]),
                UpMachine = int.Parse(parts[1]),
                Up2Machine = int.Parse(parts[2]),
                DownMachine = int.Parse(parts[3]),
                Size = int.Parse(parts[4]),
                SelectionProbability = Utils.ToFloat(parts[5])
            };
        }

        public static FlowlineConfiguration ReadInstanceFromFile(string path)
        {
            var lines = File.ReadAllLines(path);

            void FillWithFloatFromLine(out float field, int lineIx)
            {
                field = float.Parse(lines[lineIx]);
            }

            void FillWithIntFromLine(out int field, int lineIx)
            {
                field = int.Parse(lines[lineIx]);
            }

            var flc = new FlowlineConfiguration();

            // Global data
            FillWithFloatFromLine(out flc.RequiredRelativeMarginOfError, 0);
            FillWithIntFromLine(out flc.NumMachines, 1);
            FillWithIntFromLine(out flc.NumBuffers, 2);
            FillWithIntFromLine(out flc.MilkRunCycleLength, 3);

            // Machine related
            flc.Machines = new List<Machine>();
            const int firstMachineLineIx = 4;
            for (var i = 0; i < flc.NumMachines; i++)
            {
                var lineIx = firstMachineLineIx + i;
                var parts = lines[lineIx].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                flc.Machines.Add(MachineFromParts(parts));
            }

            // Buffer related
            flc.Buffers = new List<Buffer>();
            var firstBufferLineIx = firstMachineLineIx + flc.NumMachines;
            for (var i = 0; i < flc.NumBuffers; i++)
            {
                var lineIx = firstBufferLineIx + i;
                var parts = lines[lineIx].Split(" ", StringSplitOptions.RemoveEmptyEntries);
                flc.Buffers.Add(BufferFromParts(parts));
            }

            return flc;
        }
    }
}