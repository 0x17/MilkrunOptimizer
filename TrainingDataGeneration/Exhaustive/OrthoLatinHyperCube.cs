using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MilkrunOptimizer.Helpers;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.TrainingDataGeneration.Exhaustive {
    public static class OrthoLatinHyperCube {
        public static List<Sample> PickSamples(FeatureDescription[] features, int numValues,
            int subCubeSplitFactor = 2) {
            int numFeatures = features.Length;
            var featureIndices = Enumerable.Range(0, numFeatures).ToArray();

            List<double>[] ComputeFeatureValues() {
                Debug.WriteLine($"Computing feature values for {features.Length} features...");
                double[] deltas = featureIndices
                    .Select(i => (features[i].UpperBound - features[i].LowerBound) / (numValues - 1)).ToArray();
                var vals = featureIndices.Select(i => Utils.RangeCount(features[i].LowerBound, deltas[i], numValues).ToList()).ToArray();
                Debug.WriteLine("Rounding all discrete features...");
                for (int i = 0; i < vals.Length; i++) {
                    if (features[i].IsDiscrete) {
                        vals[i] = vals[i].Select(v => Math.Round(v)).ToList();
                    }
                    Debug.Assert(vals[i].Count == numValues, "Missed targeted number of values");
                }
                return vals;
            }

            double[][] ComputeSubCubeBorders() {
                var subCubeWidths = featureIndices.Select(i =>
                    Math.Round((features[i].UpperBound - features[i].LowerBound) / subCubeSplitFactor, 2)).ToArray();
                return featureIndices.Select(i =>
                    Enumerable.Range(0, subCubeSplitFactor + 1)
                        .Select(j => j * subCubeWidths[i] + features[i].LowerBound).ToArray()).ToArray();
            }

            var values = ComputeFeatureValues();
            var subCubeBorders = ComputeSubCubeBorders();

            var samples = new List<Sample>();

            var rand = new Random(23);

            int numSubCubes = (int) Math.Pow(subCubeSplitFactor, numFeatures);
            Debug.Assert(numValues >= numSubCubes,
                "Number of values must be greater or equal than the number of subcubes for each subcube to hold at least one element.");
            int numSamplesPerSubCube = (int) Math.Round(numValues / (double) numSubCubes);
            Debug.Assert(numSamplesPerSubCube * numSubCubes == numValues,
                "Total number of samples must equal the number of values.");

            List<double> GetValuesInSubcubeForFeature(List<double> valuesForFeature, double min, double max) {
                double eps = 0.0;
                List<double> filteredElems;
                do {
                    filteredElems = valuesForFeature.Where(x => x >= min - eps && x <= max + eps).ToList();
                    eps += 1.0;
                } while (filteredElems.Count == 0);

                return filteredElems;
            }

            string[] featureNames = features.Select(f => f.Name).ToArray();

            var sw = new Stopwatch();
            sw.Start();

            int ctr = 0;
            foreach (var triple in Enumerable.Range(0, numFeatures).Select(i => Enumerable.Range(0, subCubeSplitFactor))
                .CartesianProduct()) {
                var tripleArr = triple.ToArray();
                var mins = featureIndices.Select(i => subCubeBorders[i][tripleArr[i]]).ToArray();
                var maxs = featureIndices.Select(i => subCubeBorders[i][tripleArr[i] + 1]).ToArray();
                var valuesInSubcube = values.Select((valuesForFeature, ix) => GetValuesInSubcubeForFeature(valuesForFeature, mins[ix], maxs[ix])).ToArray();
                samples.AddRange(Enumerable.Range(0, numSamplesPerSubCube).Select(i => PickRandomlyAndRemove(rand, featureNames, values, valuesInSubcube)));
                var averageSamplesPerElapsedTime = (double) samples.Count / (double) sw.ElapsedMilliseconds;
                var remainingTime = numValues / averageSamplesPerElapsedTime;
                Console.Write(
                    $"\rPicked {samples.Count} samples so far... Current cube is {string.Join(",", tripleArr)} means a progress of {(double) ctr / (double) numSubCubes * 100.0}% with ETA={remainingTime / 1000.0}");
                ctr++;
            }

            Debug.Assert(samples.Count == numValues);

            return samples;
        }

        public static List<int> ExtractValues(string[] featureNames, double[] values, string prefix) {
            if (!featureNames.Any(name => name.StartsWith(prefix))) return null;
            Debug.Assert(featureNames.Length == values.Count());
            return Enumerable.Range(0, featureNames.Length).Where(ix => featureNames[ix].StartsWith(prefix))
                .Select(ix => (int) values[ix]).ToList();
        }

        public static List<float> ExtractValuesFloat(string[] featureNames, double[] values, string prefix) {
            if (!featureNames.Any(name => name.StartsWith(prefix))) return null;
            return Enumerable.Range(0, featureNames.Length).Where(ix => featureNames[ix].StartsWith(prefix))
                .Select(ix => (float) values[ix]).ToList();
        }

        public static Sample FromPick(string[] featureNames, double[] featureValues) {
            var mcl = ExtractValues(featureNames, featureValues, DefaultFeatures.MilkRunCycleLength.ToString());
            int mclValue = mcl?.First() ?? -1;
            return new Sample {
                BufferSizes = ExtractValues(featureNames, featureValues, DefaultFeatures.BufferSize.ToString()),
                ProcessingRates =
                    ExtractValuesFloat(featureNames, featureValues, DefaultFeatures.ProcessingRate.ToString()),
                MilkrunCycleLength = mclValue,
                OrderUpToLevels = RatiosToOrderUpToLevels(mclValue,
                    ExtractValuesFloat(featureNames, featureValues, DefaultFeatures.MaterialRatio.ToString()))
            };
        }

        private static List<int> RatiosToOrderUpToLevels(int milkrunCycleLength, List<float> materialRatios) {
            return materialRatios.Select(mr => (int) Math.Round(mr * milkrunCycleLength)).ToList();
        }

        public static Sample PickRandomlyAndRemove(Random rand, string[] featureNames, List<double>[] values,
            List<double>[] valuesInSubcube) {
            double ChooseAndRemove(int featureIndex) {
                Debug.Assert(valuesInSubcube[featureIndex].Count > 0);
                List<double> valuesInSubcubeForThisFeature = valuesInSubcube[featureIndex];
                int randIndex = rand.Next(0, valuesInSubcubeForThisFeature.Count);
                double elem = valuesInSubcubeForThisFeature[randIndex];
                valuesInSubcubeForThisFeature.Remove(elem);
                values[featureIndex].Remove(elem);
                return elem;
            }

            return FromPick(featureNames, Enumerable.Range(0, featureNames.Length).Select(ChooseAndRemove).ToArray());
        }
    }

    public struct FeatureDescription {
        public string Name;
        public double LowerBound, UpperBound;
        public bool IsDiscrete;
    }

    public enum DefaultFeatures {
        MilkRunCycleLength,
        ProcessingRate,
        MaterialRatio,
        BufferSize
    }
}