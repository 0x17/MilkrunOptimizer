using MilkrunOptimizer.Helpers;

namespace MilkrunOptimizer.Cluster
{
    public class JobGenerator
    {
        const string JobTemplate = @"#!/bin/bash -login
#PBS -N milkrun-simulation-batch-BATCH_NUM
#PBS -M andre.schnabel@prod.uni-hannover.de
#PBS -m a
#PBS -j oe
#PBS -l nodes=1:ppn=1
#PBS -l walltime=200:00:00
#PBS -l mem=1gb
#PBS -q all            
cd $HOME/netcoreapp3.0
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
dotnet MilkrunOptimizer.dll BatchSimulation From=SEED_LB_INCL To=SEED_UB_INCL
";

        public static void GenerateJobs(int numInstancesPerBatch = 1000, int numBatches = 1000, int offset = 0)
        {
            for (int i = 0; i < numBatches; i++)
            {
                int seedLbIncl = numInstancesPerBatch * i;
                int seedUbIncl = seedLbIncl + numInstancesPerBatch - 1;
                string ostr = JobTemplate
                    .Replace("SEED_LB_INCL", (offset + seedLbIncl).ToString())
                    .Replace("SEED_UB_INCL", (offset + seedUbIncl).ToString())
                    .Replace("BATCH_NUM", (i + 1).ToString());
                Utils.WriteUnixStyle($"job_batch_{i+1}_generated.sh", ostr);
            }
        }
    }
}