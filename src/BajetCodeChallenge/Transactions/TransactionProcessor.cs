using BajetCodeChallenge.Files;
using System.Collections.Concurrent;

namespace BajetCodeChallenge.Transactions;

public class TransactionProcessor
{
    private int WorkerCount { get; set; }

    private BlockingCollection<string> Queue { get; set; }
    
    private List<Task> Producers { get; set; }

    private List<Task<Transaction>> Consumers { get; set; }


    public TransactionProcessor()
    {
        int workerCount = Environment.ProcessorCount;
        WorkerCount = workerCount;

        Queue = [];

        Consumers = [];
        Producers = [];
    }
   
    public async Task<ProcessorResult> Run(string path, bool runWithSingleProducer = false)
    {
        if(runWithSingleProducer)
            InitializeSingleProducer(path);
        else
            // ToDo: it has the bug that it misses some lines in data file
            InitializeProducers(path);
        
        InitializeConsumers();


        await Task.WhenAll(Producers);
        Queue.CompleteAdding();

        return await GetResult();
    }

    private async Task<ProcessorResult> GetResult()
    {
        long totalCount = 0;
        decimal totalSum = 0m;
        var finalDuplicate = new Dictionary<(string, long), int>();

        await foreach (var data in Task.WhenEach(Consumers))
        {
            var partialResult = await data;

            totalCount += partialResult.Count;
            totalSum += partialResult.Sum;

            foreach (var duplicate in partialResult.Duplicates)
            {
                if (duplicate.Value <= 1)
                    continue;

                if (finalDuplicate.TryGetValue(duplicate.Key, out var exist))
                    finalDuplicate[duplicate.Key] = exist + duplicate.Value;
                else
                    finalDuplicate[duplicate.Key] = duplicate.Value;
            }
        }

        var result = new ProcessorResult(totalCount, totalSum, finalDuplicate);

        return result;
    }

    #region Producer

    private void InitializeProducers(string path)
    {
        var chunks = FileReader.GetFileChunks(path, WorkerCount);

        foreach (var chunk in chunks)
        {
            var producer = Task.Factory.StartNew(() => ReadChunk(path, chunk.Value.Start, chunk.Value.End));
            Producers.Add(producer);
        }

        return;
    }

    private void InitializeSingleProducer(string path)
    {
        var producer = Task.Factory.StartNew(() =>
        {
            using var reader = new StreamReader(path);
            
            reader.ReadLine(); // skip header

            string? nextLine;

            while ((nextLine = reader.ReadLine()) is not null)
            {
                if (string.IsNullOrWhiteSpace(nextLine))
                    continue;

                Queue.TryAdd(nextLine);
            }
        });

        Producers.Add(producer);

        return;
    }

    private void ReadChunk(string path, long start, long end)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var reader = new StreamReader(stream);

        stream.Seek(start, SeekOrigin.Begin);

        // Skip header if it's the first chunk and If not the first chunk, we need to skip the partial line
        if (start >= 0)
            reader.ReadLine();

        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            // If we're past our chunk's end and not the last chunk, stop reading
            if (stream.Position > end && end != stream.Length)
                break;

            if (!string.IsNullOrWhiteSpace(line))
            {
                Queue.TryAdd(line);
            }
        }
    }

    #endregion

    #region Consumer

    private void InitializeConsumers()
    {
        var consumers = Enumerable.Range(0, WorkerCount)
            .Select(_ => Task.Factory.StartNew(() => ReadFromQueue()))
            .ToList();

        Consumers.AddRange(consumers);

        return;
    }

    private Transaction ReadFromQueue()
    {
        var partialResult = new Transaction();

        foreach (var line in Queue.GetConsumingEnumerable())
        {
            var parts = line.Split('\t');
            var source = parts[0];
            var amount = long.Parse(parts[1]);

            partialResult.Count++;
            partialResult.Sum += amount;

            var key = (source, amount);
            if (partialResult.Duplicates.TryGetValue(key, out var duplicateCount))
                partialResult.Duplicates[key] = duplicateCount + 1;
            else
                partialResult.Duplicates[key] = 1;

        }
        return partialResult;
    }

    #endregion
}
