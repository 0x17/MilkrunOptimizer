using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.TrainingDataGeneration.Exhaustive {
    public static class OrthoLatinHyperCube {
        public static List<Sample> PickSamples(FeatureDescription[] features, int numValues, int subCubeSplitFactor = 2) {
            int numCoords = features.Length;
            var coords = Enumerable.Range(0, numCoords).ToArray();

            List<double>[] ComputeFeatureValues() {
                double[] deltas = coords.Select(i => (features[i].UpperBound - features[i].LowerBound) / (numValues-1)).ToArray();
                var vals = coords.Select(i => Utils.Range(features[i].LowerBound, features[i].LowerBound+(deltas[i]*(numValues-(features[i].IsDiscrete?1:0))), deltas[i]).ToList()).ToArray();
                for (int i = 0; i < vals.Length; i++) {
                    if (features[i].IsDiscrete) {
                        vals[i] = vals[i].Select(v => Math.Round(v)).ToList();
                    }
                }

                return vals;
            }

            double[][] ComputeSubCubeBorders() {
                var subCubeWidths = coords.Select(i => Math.Round((features[i].UpperBound - features[i].LowerBound) / subCubeSplitFactor, 2)).ToArray();
                return coords.Select(i => Enumerable.Range(0, subCubeSplitFactor + 1).Select(j => j * subCubeWidths[i] + features[i].LowerBound).ToArray()).ToArray();
            }

            var values = ComputeFeatureValues();
            var subCubeBorders = ComputeSubCubeBorders();

            var samples = new List<Sample>();
            
            var rand = new Random(23);
            
            int numSubCubes = (int) Math.Pow(subCubeSplitFactor, numCoords);
            int numSamplesPerSubCube = (int) Math.Round(numValues / (double) numSubCubes);
            Debug.Assert(numSamplesPerSubCube * numSubCubes == numValues, "Total number of samples must equal the number of values.");

            foreach (var triple in Enumerable.Range(0, numCoords).Select(i => Enumerable.Range(0, subCubeSplitFactor)).CartesianProduct()) {
                var tripleArr = triple.ToArray();
                var mins = coords.Select(i => subCubeBorders[i][tripleArr[i]]).ToArray();
                var maxs = coords.Select(i => subCubeBorders[i][tripleArr[i] + 1]).ToArray();
                samples.AddRange(Enumerable.Range(0, numSamplesPerSubCube).Select(i => PickRandomlyAndRemove(rand, features, values, mins, maxs)));
            }
            
            Debug.Assert(samples.Count == numValues);

            return samples;
        }

        public static List<int> ExtractValues(string[] featureNames, double[] values, string prefix) {
            if (!featureNames.Any(name => name.StartsWith(prefix))) return null;
            Debug.Assert(featureNames.Length==values.Length);
            return Enumerable.Range(0, featureNames.Length).Where(ix => featureNames[ix].StartsWith(prefix)).Select(ix => (int) values[ix]).ToList();
        }

        public static List<float> ExtractValuesFloat(string[] featureNames, double[] values, string prefix) {
            if (!featureNames.Any(name => name.StartsWith(prefix))) return null;
            return Enumerable.Range(0, featureNames.Length).Where(ix => featureNames[ix].StartsWith(prefix)).Select(ix => (float) values[ix]).ToList();
        }

        private static Sample FromPick(string[] featureNames, double[] featureValues) {
            var mcl = ExtractValues(featureNames, featureValues, "milkrun_cycle_length");
            return new Sample {
                BufferSizes = ExtractValues(featureNames, featureValues, "buffer_size"),
                ProcessingRates = ExtractValuesFloat(featureNames, featureValues, "processing_rate"),
                MilkrunCycleLength = mcl?.First() ?? -1,
                OrderUpToLevels = ExtractValues(featureNames, featureValues, "order_up_to_level")
            };
        }

        private static Sample PickRandomlyAndRemove(Random rand, FeatureDescription[] features, List<double>[] values, double[] mins, double[] maxs) {
            const double eps = 0.001;
            
            double ChooseAndRemove(List<double> elems, double min, double max) {
                var filteredElems = elems.Where(x => x >= min - eps && x <= max + eps).ToArray();
                int randIndex = rand.Next(0, filteredElems.Count());
                double elem = filteredElems[randIndex];
                elems.Remove(elem);
                return elem;
            }

            string[] featureNames = features.Select(f => f.Name).ToArray();
            return FromPick(featureNames, featureNames.Select((name, i) => ChooseAndRemove(values[i], mins[i], maxs[i])).ToArray());
        }
    }

    public struct FeatureDescription {
        public string Name;
        public double LowerBound, UpperBound;
        public bool IsDiscrete;
    }
}