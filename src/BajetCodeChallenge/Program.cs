using BajetCodeChallenge.Transactions;
using Microsoft.Extensions.Configuration;


var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", false, true);

var config = configuration.Build();

var filePath = config["FilePath"];

if (string.IsNullOrEmpty(filePath))
{
    Console.WriteLine("File path is not set in appsettings.json");
    return;
}

if (!File.Exists(filePath))
{
    Console.WriteLine("The file is not exist!.");
    return;
}


var processor = new TransactionProcessor();
var processorResult = await processor.Run(filePath, true);

Console.WriteLine($"Total transactions: {processorResult.TotalCount}");
Console.WriteLine($"Total Duplicated transactions: {processorResult.Duplicates.Count}");
Console.WriteLine($"Sum of all amounts: {processorResult.TotalSum:N0}");

Console.WriteLine("\nDuplicate transactions (source_account, amount, count):");
foreach (var kv in processorResult.Duplicates.OrderByDescending(x => x.Value))
{
    Console.WriteLine($"  {kv.Key.source_name}\t{kv.Key.amount:N0}\tCount: {kv.Value}");
}