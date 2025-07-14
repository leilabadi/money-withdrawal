namespace Moneybox.App.Domain.Model;

public class TransactionResult<T>(T transaction) where T : Transaction
{
    public T Transaction { get; init; } = transaction;
    public bool IsSuccessful { get; private set; } = true;
    public string? Error { get; private set; } = null;

    public TransactionResult(T transaction, string error) : this(transaction)
    {
        IsSuccessful = false;
        Error = error;
    }
}
