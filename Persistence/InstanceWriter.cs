using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;
using Buffer = MilkrunOptimizer.Model.Buffer;

namespace MilkrunOptimizer.Persistence
{
    public static class InstanceWriter
    {
        public static void ToJson(FlowlineConfiguration flc, string path)
        {
            Utils.SaveObjectAsJson(flc, path);
        }

        public static void WriteInstanceToFile(FlowlineConfiguration flc, string path)
        {
            var lines = new List<string>();

            void AddLine(int v)
            {
                lines.Add(v.ToString());
            }

            void AddLineF(float v)
            {
                lines.Add(v.ToString(CultureInfo.InvariantCulture));
            }

            AddLineF(flc.RequiredRelativeMarginOfError);
            AddLine(flc.NumMachines);
            AddLine(flc.NumBuffers);
            AddLine(flc.MilkRunCycleLength);

            for (var i = 0; i < flc.NumMachines; i++) lines.Add(MachineToLine(flc.Machines[i]));

            for (var i = 0; i < flc.NumBuffers; i++) lines.Add(BufferToLine(flc.Buffers[i]));

            lines.Add("\n0\n");

            File.WriteAllLines(path, lines);
        }

        private static string BufferToLine(Buffer buffer)
        {
            int[] firstEntries =
            {
                buffer.NumMachinesSurrounding,
                buffer.UpMachine,
                buffer.Up2Machine,
                buffer.DownMachine,
                buffer.Size
            };
            var lastEntry = buffer.SelectionProbability;

            var values = Array.ConvertAll(firstEntries, v => v.ToString()).ToList();
            values.Add(lastEntry.ToString(CultureInfo.InvariantCulture));

            return string.Join(" ", values);
        }

        private static string MachineToLine(Machine machine)
        {
            float[] floatEntries =
            {
                machine.FailRate,
                machine.ReplacementArrivalRate,
                machine.ProcessingRate,
                machine.CoefficientVariationSquared
            };

            var values = new List<string> {machine.OperationalUnits.ToString()};
            values.AddRange(Array.ConvertAll(floatEntries, v => v.ToString(CultureInfo.InvariantCulture)));
            values.Add(machine.OrderUpToMilkLevel.ToString());

            return string.Join(" ", values);
        }
    }
}