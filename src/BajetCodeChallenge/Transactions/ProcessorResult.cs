namespace BajetCodeChallenge.Transactions;

public record ProcessorResult(long TotalCount, decimal TotalSum , Dictionary<(string source_name, long amount), int> Duplicates)
{

}
