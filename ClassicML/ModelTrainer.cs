using System;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.ClassicML {
    public static class ModelTrainer {
        public static ITransformer TrainModelWithData(MLContext context, TrainingData train, TrainingData validation,
            out DataViewSchema inputSchema) {
            //var trainer = context.Regression.Trainers.FastForest();
            //var trainer = context.Regression.Trainers.FastTreeTweedie();
            var trainer = context.Regression.Trainers.LightGbm();
            var trainingView = ToDataView(context, train);
            inputSchema = trainingView.Schema;
            var validationView = ToDataView(context, validation);
            var transformer = validation != null ? trainer.Fit(trainingView, validationView) : trainer.Fit(trainingView);
            var predictions = transformer.Transform(validationView);
            RegressionMetrics trainedModelMetrics = context.Regression.Evaluate(predictions);
            Console.WriteLine("Validation loss = {0}", trainedModelMetrics.LossFunction);
            return transformer;
        }

        internal class MlSample {
            [LoadColumn(0, 10)]
            [VectorType(11)]
            [ColumnName("Features")]
            public float[] Features;
            
            [LoadColumn(11)]
            [ColumnName("Label")]
            public float ProductionRate;
        }

        internal static MlSample ConvertToMlSample(Sample s) {
            return new MlSample {
                Features = s.BufferSizes.Select(size => (float)size).Concat(s.MaterialRatios).Concat(s.ProcessingRates).ToArray(),
                ProductionRate = s.ProductionRate
            };
        }

        internal static IDataView ToDataView(MLContext context, TrainingData data) {
            var convertedSamples = data.Samples.Select(ConvertToMlSample);
            return context.Data.LoadFromEnumerable(convertedSamples);
        }
    }
}