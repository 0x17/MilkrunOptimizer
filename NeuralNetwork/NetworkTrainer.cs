using System;
using System.Collections.Generic;
using System.Linq;
using Keras.Layers;
using Keras.Models;
using MilkrunOptimizer.Model;
using Numpy;

namespace MilkrunOptimizer.NeuralNetwork
{
    public static class NetworkTrainer
    {
        public const int NumFeatures = 4 + 4 + 3;

        public static NDarray XsFromSample(Sample sample)
        {
            var td = new TrainingData {Samples = new List<Sample> {sample}};
            return XsFromTrainingData(td);
        }

        public static NDarray XsFromTrainingData(TrainingData data)
        {
            var numSamples = data.Samples.Count;
            var xsBase = new float[numSamples, NumFeatures];

            for (var i = 0; i < numSamples; i++)
            for (var j = 0; j < NumFeatures; j++)
            {
                var sample = data.Samples[i];
                xsBase[i, j] = j < 4 ? sample.ProcessingRates[j] :
                    j < 8 ? sample.MaterialRatios[j - 4] : sample.BufferSizes[j - 8];
            }

            return np.array(xsBase);
        }

        public static TrainValidationData Split(TrainingData data, float trainPercentage = 0.5f, bool shuffle = false)
        {
            var training = new TrainingData {Samples = new List<Sample>()};
            var validation = new TrainingData {Samples = new List<Sample>()};

            var lastTrainIndex = (int) Math.Round(data.Samples.Count * trainPercentage);
            for (var i = 0; i < data.Samples.Count; i++)
            {
                var sample = data.Samples[i];
                if (i <= lastTrainIndex)
                    training.Samples.Add(sample);
                else
                    validation.Samples.Add(sample);
            }

            return new TrainValidationData
            {
                Training = training,
                Validation = validation
            };
        }

        public static Sequential TrainNetworkWithData(TrainingData data)
        {
            var xs = XsFromTrainingData(data);
            var ysBase = data.Samples.Select(sample => sample.ProductionRate).ToArray();
            NDarray ys = np.array(ysBase);

            var model = new Sequential();
            model.Add(new Dense(256, NumFeatures, "relu", kernel_initializer: "uniform"));
            var hiddenLayerSizes = new List<int> {128, 64, 32, 16};
            foreach (var size in hiddenLayerSizes)
                model.Add(new Dense(size, activation: "relu", kernel_initializer: "uniform"));
            model.Add(new Dense(1, activation: "sigmoid", kernel_initializer: "uniform"));
            model.Compile("RMSprop", "mape", new string[] { });
            model.Summary();

            model.Fit(xs, ys, 128, 100, 2, shuffle: false);
            model.Save("trained_model.hdf5");

            return model;
        }

        public static BaseModel LoadFromDisk(string path)
        {
            return BaseModel.LoadModel(path);
        }
    }
}