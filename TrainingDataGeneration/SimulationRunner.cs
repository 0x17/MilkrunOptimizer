using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Persistence;

namespace MilkrunOptimizer.TrainingDataGeneration
{
    public static class SimulationRunner
    {
        private static readonly Dictionary<string, string> simulationBinaryPaths = new Dictionary<string, string>
        {
            {"Darwin", "/Users/andreschnabel/Seafile/Dropbox/HelberSimulation/FlowLineOptimizer/clustersim/Simulation"},
            {
                "Windows",
                "C:\\Users\\Andre\\Seafile\\Dropbox\\HelberSimulation\\FlowLineOptimizer\\clustersim\\SimulationWindows.exe"
            },
            {"Linux", "./Simulation"}
        };

        public static float ProductionRateForConfiguration(FlowlineConfiguration flc)
        {
            
            string tmpFilenameBase = $"temp_hash_{flc.GetHashCode()}";
            InstanceWriter.WriteInstanceToFile(flc, tmpFilenameBase + ".mrn");
            RunSimulationExecutable(tmpFilenameBase);
            var productionRate = ResultParser.ProductionRateFromResultFile(tmpFilenameBase + ".stb");
            var extensions = new List<string> {".mrn", ".stb"};
            extensions.ForEach(ext => RetryDelete(tmpFilenameBase + ext));
            return productionRate;
        }

        private static void RetryDelete(string path, int maxRetries = 100)
        {
            if (maxRetries == 0)
                return;
            try
            {
                File.Delete(path);
            }
            catch (Exception)
            {
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine($"Retrying delete operation for {path}, remaining retry count is {maxRetries}...");
                RetryDelete(path, maxRetries-1);
            }
        }

        private static string GetPathForBinaryForThisSystem()
        {
            var osNameAndVersion = RuntimeInformation.OSDescription;
            return simulationBinaryPaths.First(pair => osNameAndVersion.Contains(pair.Key)).Value;
        }

        private static void RunSimulationExecutable(string lineBaseFilename)
        {
            var binaryPath = GetPathForBinaryForThisSystem();
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = binaryPath,
                    Arguments = lineBaseFilename,
                    RedirectStandardOutput = true
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}