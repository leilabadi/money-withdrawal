namespace Moneybox.App.Domain.Model;

public class LedgerEntry(Guid accountId, decimal transactionAmount, DateTime timestamp, string description, Guid? relatedTransactionId = null)
{
    public Guid Id { get; } = Guid.NewGuid();
    public Guid AccountId { get; } = accountId;
    public decimal TransactionAmount { get; } = transactionAmount;
    public DateTime Timestamp { get; } = timestamp;
    public string Description { get; init; } = description;
    public Guid? RelatedTransactionId { get; init; } = relatedTransactionId;

    public LedgerEntryType EntryType
    {
        get
        {
            // Positive for credit, negative for debit
            return TransactionAmount switch
            {
                > 0 => LedgerEntryType.Credit,
                < 0 => LedgerEntryType.Debit,
                _ => throw new InvalidOperationException("Ledger entry amount cannot be zero.")
            };
        }
    }
}
