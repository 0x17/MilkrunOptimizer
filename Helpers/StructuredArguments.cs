using System.Collections.Generic;
using System.Linq;

namespace MilkrunOptimizer.Helpers {
    public struct StructuredArguments {
        private readonly string _action;
        private readonly Dictionary<string, string> _pairs;

        public static StructuredArguments FromStrings(string[] args) {
            return new StructuredArguments(args[0],
                args.Skip(1).ToDictionary(arg => arg.Split("=")[0], arg => arg.Split("=")[1]));
        }

        public StructuredArguments(string action, Dictionary<string, string> pairs) {
            _action = action;
            _pairs = pairs;
        }

        public string GetAction() {
            return _action;
        }

        public int AsInt(string argName) {
            return int.Parse(_pairs[argName]);
        }

        public int AsIntOrDefault(string argName, int defaultValue) {
            return HasArg(argName) ? AsInt(argName) : defaultValue;
        }

        public string AsStringOrDefault(string argName, string defaultValue) {
            return HasArg(argName) ? AsString(argName) : defaultValue;
        }

        public string AsString(string argName) {
            return _pairs[argName];
        }

        public bool HasArg(string argName) {
            return _pairs.ContainsKey(argName);
        }
    }
}