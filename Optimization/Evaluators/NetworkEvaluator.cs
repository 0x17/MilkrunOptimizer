﻿using Keras.Models;
using localsolver;
using MilkrunOptimizer.Model;
using MilkrunOptimizer.NeuralNetwork;

namespace MilkrunOptimizer.Optimization.Evaluators {
    internal class NetworkEvaluator : BaseEvaluator {
        private readonly ProductionRatePredictor _predictor;

        public NetworkEvaluator(MilkrunBufferAllocationProblem problem, BaseModel model) : base(problem) {
            _predictor = new ProductionRatePredictor(model);
        }

        public override double Evaluate(LSNativeContext context) {
            ExtractDataFromContext(context);
            return _predictor.PredictConfig(Flc);
        }
    }
}