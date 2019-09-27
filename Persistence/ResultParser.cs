using System;
using System.IO;
using MilkrunOptimizer.Helpers;

namespace MilkrunOptimizer.Persistence
{
    public static class ResultParser
    {
        public static float ProductionRateFromResultFile(string path)
        {
            var lines = File.ReadAllLines(path);
            const int firstBufferLineIndex = 2;
            const int bufferProductionRateColumnIndex = 5;
            return Utils.ToFloat(
                lines[firstBufferLineIndex].Split(" ", StringSplitOptions.RemoveEmptyEntries)[
                    bufferProductionRateColumnIndex]);
        }
    }
}