using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Cluster
{
    public class MergeResultFiles
    {
        public static TrainingData MergeDataInPath(string path, string extension, bool showProgress = true)
        {
            var filenames = Directory.GetFiles(path).Where(fn => fn.EndsWith(extension)).OrderBy(fn => fn).ToList();
            var trainingDatas = filenames.Select(Persistence.TrainingDataPersistence.LoadFromDisk);
            var samples = new List<Sample>();
            int ctr = 0;
            int totalSampleCount = 0;
            foreach (var td in trainingDatas)
            {
                if(showProgress)
                    Console.WriteLine($"Adding {td.Samples.Count} samples to the merged dataset. Progress is {(float)ctr/(float)filenames.Count*100.0f}%...");
                samples.AddRange(td.Samples);
                ctr++;
                totalSampleCount += td.Samples.Count;
            }
            Console.WriteLine($"Merged {totalSampleCount} samples in total.");
            return new TrainingData {Samples = samples};
        } 
    }
}