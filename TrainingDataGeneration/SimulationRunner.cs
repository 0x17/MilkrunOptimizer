using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
            const string tmpFilenameBase = "temp";
            InstanceWriter.WriteInstanceToFile(flc, tmpFilenameBase + ".mrn");
            RunSimulationExecutable(tmpFilenameBase);
            var productionRate = ResultParser.ProductionRateFromResultFile(tmpFilenameBase + ".stb");
            var extensions = new List<string> {".mrn", ".stb"};
            extensions.ForEach(ext => File.Delete(tmpFilenameBase + ext));
            return productionRate;
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
                    Arguments = lineBaseFilename
                }
            };
            proc.Start();
            proc.WaitForExit();
        }
    }
}