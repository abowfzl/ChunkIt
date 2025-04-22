using BajetCodeChallenge.Transactions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace BajetCodeChallenge.Benchmark;

[SimpleJob(RunStrategy.ColdStart, iterationCount: 30)]
[MinColumn, MaxColumn, MeanColumn]
public class TransactionBenchmark
{
    [Params("./sample.txt", "./sample_100K.txt", "./sample_1M.txt")]
    public string Path { get; set; }

    [Benchmark]
    public async Task<ProcessorResult> RunSinle()
    {
        var processor = new TransactionProcessor();
        var processorResult = await processor.Run(Path, true);

        return processorResult;
    }    
    
    [Benchmark]
    public async Task<ProcessorResult> RunMultiple()
    {
        var processor = new TransactionProcessor();
        var processorResult = await processor.Run(Path);

        return processorResult;
    }

}
