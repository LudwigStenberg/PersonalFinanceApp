namespace PersonalFinanceApp.Services.Implementation;

public class TransactionIdGenerator : IIdGenerator
{
    private const string TransactionPrefix = "TRN";
    private readonly Dictionary<string, int> _dailyTransactionCount = new Dictionary<string, int>();


    public string GenerateId()
    {
        string dateKey = DateTime.Now.ToString("yyyyMMdd");

        if (!_dailyTransactionCount.TryGetValue(dateKey, out int termKey))
        {
            termKey = 0;
        }
        termKey++;
        _dailyTransactionCount[dateKey] = termKey;

        return $"{TransactionPrefix}{dateKey}{termKey:000}";
    }


}



