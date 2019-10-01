using Keras.Models;

namespace MilkrunOptimizer.NeuralNetwork {
    public static class ModelPersistence {
        public static BaseModel LoadFromDisk(string path) {
            return BaseModel.LoadModel(path);
        }
    }
}