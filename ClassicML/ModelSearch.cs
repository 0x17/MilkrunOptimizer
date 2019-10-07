using System;
using Microsoft.ML;
using Microsoft.ML.AutoML;
using Microsoft.ML.Data;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.ClassicML {
    public static class ModelSearch {
        
        private class Handler : IProgress<RunDetail<RegressionMetrics>> {
            public void Report(RunDetail<RegressionMetrics> value) {
                Console.Write($"\rProgress... Trainer={value.TrainerName}, MAPE={value.ValidationMetrics.MeanAbsoluteError}, Runtime={value.RuntimeInSeconds}");
            }
        };
        
        public static void AutoMlOnDataset(MLContext context, TrainingData train, TrainingData validation) {
            var regExpSettings = new RegressionExperimentSettings {
                MaxExperimentTimeInSeconds = 60*5,
                OptimizingMetric = RegressionMetric.MeanAbsoluteError,
                CacheDirectory = null,
            };
            var experiment = context.Auto().CreateRegressionExperiment(regExpSettings);
            IProgress<RunDetail<RegressionMetrics>> progressHandler = new Handler();
            //ModelTrainer.ToDataView(context, validation)
            var experimentResults = experiment.Execute(ModelTrainer.ToDataView(context, train), progressHandler:progressHandler);
            foreach (var res in experimentResults.RunDetails) {
                if(res.TrainerName != null && res.ValidationMetrics != null)
                    Console.WriteLine($"Trainer={res.TrainerName}; MAE={res.ValidationMetrics.MeanAbsoluteError}");
            }
            var best = experimentResults.BestRun;
            Console.WriteLine($"Lé best trainer is {best.TrainerName} with MAE={best.ValidationMetrics.MeanAbsoluteError}");
        }
    }
}