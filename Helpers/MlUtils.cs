using System;
using System.Collections.Generic;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Helpers {
    public static class MlUtils {
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
    }
}