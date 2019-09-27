using System.IO;
using MilkrunOptimizer.Model;
using ProtoBuf;

namespace MilkrunOptimizer.Persistence
{
    public class TrainingDataPersistence
    {
        public static TrainingData LoadFromDisk(string path)
        {
            using var file = File.OpenRead(path);
            return Serializer.Deserialize<TrainingData>(file);
        }

        public static void SaveToDisk(TrainingData data, string path)
        {
            using var file = File.Create(path);
            Serializer.Serialize(file, data);
        }
    }
}