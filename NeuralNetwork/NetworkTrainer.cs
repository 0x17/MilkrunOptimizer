using System;
using System.Collections.Generic;
using System.Linq;
using Keras.Layers;
using Keras.Models;
using MilkrunOptimizer.Model;
using Numpy;

namespace MilkrunOptimizer.NeuralNetwork {
    public static class NetworkTrainer {
        public const int NumFeatures = 4 + 4 + 3;

        public static NDarray XsFromSample(Sample sample) {
            var td = new TrainingData {Samples = new List<Sample> {sample}};
            return XsFromTrainingData(td);
        }

        public static NDarray XsFromTrainingData(TrainingData data) {
            var numSamples = data.Samples.Count;
            var xsBase = new float[numSamples, NumFeatures];

            for (var i = 0; i < numSamples; i++) {
                for (var j = 0; j < NumFeatures; j++) {
                    var sample = data.Samples[i];
                    xsBase[i, j] = j < 4 ? sample.ProcessingRates[j] :
                        j < 8 ? sample.MaterialRatios[j - 4] : sample.BufferSizes[j - 8];
                }
            }
            return np.array(xsBase);
        }

        public static TrainValidationData Split(TrainingData data, float trainPercentage = 0.5f, bool shuffle = false) {
            var training = new TrainingData {Samples = new List<Sample>()};
            var validation = new TrainingData {Samples = new List<Sample>()};

            var lastTrainIndex = (int) Math.Round(data.Samples.Count * trainPercentage);
            for (var i = 0; i < data.Samples.Count; i++) {
                var sample = data.Samples[i];
                if (i <= lastTrainIndex)
                    training.Samples.Add(sample);
                else
                    validation.Samples.Add(sample);
            }

            return new TrainValidationData {
                Training = training,
                Validation = validation
            };
        }
        
        private struct KerasTrainValidationData {
            public NDarray TrainXs, TrainYs;
            public NDarray ValidationXs, ValidationYs;
        }

        public static Sequential TrainNetworkWithData(TrainingData train, TrainingData validation = null) {
            const int batchSize = 1000;
            const int epochs = 100;
            const int verbose = 2;
            const bool shuffle = false;
            
            var data = TrainValidationSamplesToNumPyTypes(train, validation);
            
            Sequential BuildNetworkTopology() {
                var dnn = new Sequential();
                dnn.Add(new Dense(200, NumFeatures, "relu", kernel_initializer: "uniform"));
                var hiddenLayerSizes = new List<int> {100, 100, 100, 100, 100, 100, 100, 100, 100};
                foreach (var size in hiddenLayerSizes)
                    dnn.Add(new Dense(size, activation: "relu", kernel_initializer: "uniform"));
                dnn.Add(new Dense(1, activation: "sigmoid", kernel_initializer: "uniform"));
                return dnn;
            }
            
            var model = BuildNetworkTopology();
            model.Compile("adam", "mape", new string[] { });
            model.Summary();
            
            if (validation == null) {
                model.Fit(data.TrainXs, data.TrainYs, batchSize, epochs, verbose, shuffle: shuffle);
            }
            else {
                var validationData = new[] {data.ValidationXs, data.ValidationYs};
                model.Fit(data.TrainXs, data.TrainYs, batchSize, epochs, verbose, shuffle: shuffle, validation_data: validationData);
            }

            return model;
        }

        private static KerasTrainValidationData TrainValidationSamplesToNumPyTypes(TrainingData train, TrainingData validation) {
            var trainYsBase = train.Samples.Select(sample => sample.ProductionRate).ToArray();
            KerasTrainValidationData data = new KerasTrainValidationData {
                TrainXs = XsFromTrainingData(train),
                TrainYs = np.array(trainYsBase)
            };

            if (validation != null) {
                data.ValidationXs = XsFromTrainingData(validation);
                var validationYsBase = validation.Samples.Select(sample => sample.ProductionRate).ToArray();
                data.ValidationYs = np.array(validationYsBase);
            }

            return data;
        }
    }
}