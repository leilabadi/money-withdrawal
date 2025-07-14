using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public class MoneyWithdrawalService(INotificationService notificationService) : IMoneyWithdrawalService
{
    public TransactionResult<MoneyWithdrawalTransaction> WithdrawMoney(MoneyWithdrawalTransaction transaction)
    {
        var sourceAccount = transaction.SourceAccount;

        var fromBalance = sourceAccount.Balance - transaction.Amount;
        if (fromBalance < 0m)
        {
            return new TransactionResult<MoneyWithdrawalTransaction>(transaction, "Insufficient funds to make withdrawal");
        }

        if (fromBalance < Account.LowFundsThreshold)
        {
            notificationService.NotifyFundsLow(sourceAccount.User.Email);
        }

        sourceAccount.Balance -= transaction.Amount;
        sourceAccount.Withdrawn -= transaction.Amount;

        return new TransactionResult<MoneyWithdrawalTransaction>(transaction);
    }
}
