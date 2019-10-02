using System;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.ClassicML {
    public class ModelSearch {
        public static void AutoMlOnDataset(MLContext context, TrainingData train, TrainingData validation) {
            var regExpSettings = new RegressionExperimentSettings {
                MaxExperimentTimeInSeconds = 120,
                OptimizingMetric = RegressionMetric.MeanAbsoluteError,
                CacheDirectory = null,
            };
            var experiment = context.Auto().CreateRegressionExperiment(regExpSettings);
            ExperimentResult<RegressionMetrics> experimentResults = experiment.Execute(ModelTrainer.ToDataView(context, train));//, ModelTrainer.ToDataView(context, validation));
            Console.WriteLine(experimentResults.BestRun.ValidationMetrics.MeanAbsoluteError);
        }
    }
}