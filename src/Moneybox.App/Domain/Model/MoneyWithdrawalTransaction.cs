namespace Moneybox.App.Domain.Model;

public class MoneyWithdrawalTransaction(decimal amount, DateTime date, Account sourceAccount)
    : Transaction(amount, date)
{
    public Account SourceAccount { get; init; } = sourceAccount;
}
