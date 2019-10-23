﻿using System;
using System.Collections.Generic;
using System.Linq;
using Keras.Callbacks;
using Keras.Layers;
using Keras.Models;
using MilkrunOptimizer.Model;
using Numpy;

namespace MilkrunOptimizer.NeuralNetwork {
    public static class NetworkTrainer {
        // processing rates 4x, order up to levels 4x, buffer sizes 3x, milk run cycle length 1x
        public const int NumFeatures = 4 + 4 + 3 + 1;

        public static NDarray XsFromSample(Sample sample) {
            var td = new TrainingData {Samples = new List<Sample> {sample}};
            return XsFromTrainingData(td);
        }

        public static NDarray XsFromTrainingData(TrainingData data) {
            var numSamples = data.Samples.Count;
            var xsBase = new float[numSamples, NumFeatures];

            for (var i = 0; i < numSamples; i++)
            for (var j = 0; j < NumFeatures; j++) {
                var sample = data.Samples[i];
                xsBase[i, j] = j < 4 ? sample.ProcessingRates[j] :
                    j < 8 ? sample.OrderUpToLevels[j - 4] :
                    j < 8+3 ? sample.BufferSizes[j - 8] : sample.MilkrunCycleLength;
            }

            return np.array(xsBase);
        }

        public static Sequential TrainNetworkWithData(TrainingData train, TrainingData validation = null) {
            // Disable gpu computing for dense network (due to limited parallelization possibilities and empirically observed negative speedup)
            Environment.SetEnvironmentVariable("CUDA_VISIBLE_DEVICES", "-1");
            
            const int batchSize = 128;
            const int epochs = 500;
            const int verbose = 2;
            const bool shuffle = false;

            var data = TrainValidationSamplesToNumPyTypes(train, validation);

            Sequential BuildNetworkTopology() {
                var dnn = new Sequential();
                dnn.Add(new Dense(512, NumFeatures, "relu", kernel_initializer: "uniform"));
                var hiddenLayerSizes = new List<int> {256, 128, 64, 32, 16};
                foreach (var size in hiddenLayerSizes)
                    dnn.Add(new Dense(size, activation: "relu", kernel_initializer: "uniform"));
                dnn.Add(new Dense(1, activation: "sigmoid", kernel_initializer: "uniform"));
                return dnn;
            }

            var model = BuildNetworkTopology();
            model.Compile("adam", "mse", new string[] { });
            model.Summary();

            var checkpoint = new ModelCheckpoint("best_model.hdf5");
            var callbacks = new Callback[] { checkpoint };

            if (validation == null) {
                model.Fit(data.TrainXs, data.TrainYs, batchSize, epochs, verbose, shuffle: shuffle, callbacks:callbacks);
            }
            else {
                var validationData = new[] {data.ValidationXs, data.ValidationYs};
                model.Fit(data.TrainXs, data.TrainYs, batchSize, epochs, verbose, shuffle: shuffle,
                    validation_data: validationData, callbacks:callbacks);
            }

            return model;
        }

        private static KerasTrainValidationData TrainValidationSamplesToNumPyTypes(TrainingData train,
            TrainingData validation) {
            var trainYsBase = train.Samples.Select(sample => sample.ProductionRate).ToArray();
            var data = new KerasTrainValidationData {
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

        private struct KerasTrainValidationData {
            public NDarray TrainXs, TrainYs;
            public NDarray ValidationXs, ValidationYs;
        }
    }
}