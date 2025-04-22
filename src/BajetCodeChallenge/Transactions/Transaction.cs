namespace BajetCodeChallenge.Transactions;

class Transaction
{
    public long Count;
    public decimal Sum;
    public Dictionary<(string source_account, long amount), int> Duplicates = [];
}
