namespace Moneybox.App.Domain.Model;

public record TransactionResult<T> where T : PaymentTransaction
{
    public T Transaction { get; }
    public bool IsSuccessful { get; }
    public string? ErrorMessage { get; }

    private TransactionResult(T transaction, bool isSuccessful, string? errorMessage = null)
    {
        Transaction = transaction;
        IsSuccessful = isSuccessful;
        ErrorMessage = errorMessage;
    }

    public static TransactionResult<T> Success(T transaction) => new(transaction, true);

    public static TransactionResult<T> Failure(T transaction, string errorMessage) => new(transaction, false, errorMessage);
}
