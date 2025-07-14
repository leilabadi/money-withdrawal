namespace Moneybox.App.Domain.Model;

public class TransactionFactory
{
    public static MoneyTransferTransaction CreateMoneyTransferTransaction(decimal amount, Account source, Account destination)
    {
        return new MoneyTransferTransaction(amount, DateTime.UtcNow, source, destination);
    }

    public static MoneyWithdrawalTransaction CreateMoneyWithdrawalTransaction(decimal amount, Account account)
    {
        return new MoneyWithdrawalTransaction(amount, DateTime.UtcNow, account);
    }
}
