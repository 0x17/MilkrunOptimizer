using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace MilkrunOptimizer.Helpers
{
    public static class Utils
    {
        public static float ToFloat(string s)
        {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public static float Uniform(Random rand, float lb, float ub)
        {
            var num = rand.NextDouble();
            return (float) (lb + (ub - lb) * num);
        }

        public static void SaveObjectAsJson<T>(T obj, string path)
        {
            var dcjs = new DataContractJsonSerializer(typeof(T));
            var fs = new FileStream(path, FileMode.Create);
            dcjs.WriteObject(fs, obj);
            fs.Close();
        }

        public static T LoadObjectFromJson<T>(string path)
        {
            var dcjs = new DataContractJsonSerializer(typeof(T));
            var fs = new FileStream(path, FileMode.Open);
            var obj = (T) dcjs.ReadObject(fs);
            fs.Close();
            return obj;
        }

        public static void WriteUnixStyle(string path, string contents)
        {
            using TextWriter file = new StreamWriter(path);
            file.NewLine = "\n";
            file.Write(contents.Replace("\r\n", "\n"));
        }
        
        private static Random rand = new Random(23);

        public static void SetSeed(int seed)
        {
            rand = new Random(seed);
        }

        public static int RandInt(int lbIncl, int ubIncl)
        {
            return rand.Next(lbIncl, ubIncl+1);
        }

        public static double RandFloat()
        {
            return (float)rand.NextDouble();
        }
    }
}