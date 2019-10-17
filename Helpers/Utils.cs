using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Helpers {
    public static class Utils {
        private static Random rand = new Random(23);

        public static float ToFloat(string s) {
            return float.Parse(s, CultureInfo.InvariantCulture);
        }

        public static float Uniform(Random rand, float lb, float ub) {
            var num = rand.NextDouble();
            return (float) (lb + (ub - lb) * num);
        }

        public static void SaveObjectAsJson<T>(T obj, string path) {
            var dcjs = new DataContractJsonSerializer(typeof(T));
            var fs = new FileStream(path, FileMode.Create);
            dcjs.WriteObject(fs, obj);
            fs.Close();
        }

        public static T LoadObjectFromJson<T>(string path) {
            var dcjs = new DataContractJsonSerializer(typeof(T));
            var fs = new FileStream(path, FileMode.Open);
            var obj = (T) dcjs.ReadObject(fs);
            fs.Close();
            return obj;
        }

        public static void WriteUnixStyle(string path, string contents) {
            using TextWriter file = new StreamWriter(path);
            file.NewLine = "\n";
            file.Write(contents.Replace("\r\n", "\n"));
        }

        public static void SetSeed(int seed) {
            rand = new Random(seed);
        }

        public static int RandInt(int lbIncl, int ubIncl) {
            return rand.Next(lbIncl, ubIncl + 1);
        }

        public static double RandFloat() {
            return (float) rand.NextDouble();
        }

        public static string NameOfLocalActionFunction(string parentFunctionName, Action action) {
            var rx = new Regex(@"\<" + parentFunctionName + @"\>g__(\w+)\|\d+", RegexOptions.Compiled);
            var fullName = action.GetMethodInfo().Name;
            var m = rx.Match(fullName);
            return m.Groups[1].Value;
        }

        public static List<T> Shuffle<T>(List<T> list) {
            return Permutation(list.Count).Select(ix => list[ix]).ToList();
        }

        public static List<int> Permutation(int n) {
            List<int> perm = new List<int>();
            for (int i = 0; i < n; i++) {
                int x;
                do {
                    x = RandInt(0, n - 1);
                } while (perm.Contains(x));

                perm.Add(x);
            }

            return perm;
        }

        public static IEnumerable<double> Range(double lbIncl, double ubIncl, double step) {
            for (double value = lbIncl; value <= ubIncl; value += step) {
                yield return value;
            }
        }

        public static IEnumerable<double> RangeCount(double lbIncl, double step, int count) {
            for (int i = 0; i < count; i++) {
                yield return lbIncl + i * step;
            }
        }

        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences) {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] {Enumerable.Empty<T>()};
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] {item}));
        }
    }
}