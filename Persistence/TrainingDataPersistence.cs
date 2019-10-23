using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MilkrunOptimizer.Model;
using ProtoBuf;

namespace MilkrunOptimizer.Persistence {
    public static class TrainingDataPersistence {
        
        private static Sample ParseCsvLine(string line) {
            float[] parts = line.Split(",").Select(float.Parse).ToArray();
            int[] prateIndices = {3, 7, 11, 15};
            int[] orderUpToIndices = {5, 9, 13, 17};
            int[] bufferSizeIndices = {6, 10, 14};
            return new Sample {
                ProcessingRates = prateIndices.Select(i => parts[i]).ToList(),
                OrderUpToLevels = orderUpToIndices.Select(i => (int)parts[i]).ToList(),
                BufferSizes = bufferSizeIndices.Select(i => (int)parts[i]).ToList(),
                MilkrunCycleLength = (int)parts[2],
                ProductionRate = parts.Last()
            };
        }

        public static TrainingData ParseCsv(string path) {
            List<Sample> samples = new List<Sample>();
            using (StreamReader sr = File.OpenText(path))
            {
                string line = String.Empty;
                int ctr = 0;
                while ((line = sr.ReadLine()) != null) {
                    if (ctr > 0) {
                        samples.Add(ParseCsvLine(line));
                    }
                    ctr++;
                }
            }

            return new TrainingData {
                Samples = samples
            };
        }

        public static TrainingData LoadFromDisk(string path) {
            using var file = File.OpenRead(path);
            return Serializer.Deserialize<TrainingData>(file);
        }

        public static void SaveToDisk(TrainingData data, string path) {
            using var file = File.Create(path);
            Serializer.Serialize(file, data);
        }
    }
}