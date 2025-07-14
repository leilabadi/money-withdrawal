namespace Moneybox.App.Domain.Model;

public class MoneyTransferTransaction(decimal amount, DateTime date, Account sourceAccount, Account destinationAccount)
    : Transaction(amount, date)
{
    public Account SourceAccount { get; init; } = sourceAccount;
    public Account DestinationAccount { get; init; } = destinationAccount;
}
