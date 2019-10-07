using System.Linq;
using MilkrunOptimizer.Model;

namespace MilkrunOptimizer.Optimization {
    public abstract class BaseProductionRatePredictor {
        public float PredictConfig(FlowlineConfiguration config) {
            return Predict(config.ToSample());
        }

        public abstract float Predict(Sample sample);
    }
}