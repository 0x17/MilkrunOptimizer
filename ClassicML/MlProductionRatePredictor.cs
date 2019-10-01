using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.Optimization;

namespace MilkrunOptimizer.ClassicML {
    public class MlProductionRatePredictor : BaseProductionRatePredictor {

        private readonly MLContext _context = new MLContext(23);
        private readonly ITransformer _model;

        public MlProductionRatePredictor(string path) {
            this._model = _context.Model.Load(path, out _);
        }

        public override float Predict(Sample sample) {
            var convertedSample = new [] {ModelTrainer.ConvertToMlSample(sample)};
            var sampleView = _context.Data.LoadFromEnumerable(convertedSample);
            var res = _model.Transform(sampleView);
            return res.GetColumn<float>("Score").First();
        }
    }
}