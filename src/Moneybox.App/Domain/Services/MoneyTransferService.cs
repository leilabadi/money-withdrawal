using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public class MoneyTransferService(INotificationService notificationService) : IMoneyTransferService
{
    public TransactionResult<MoneyTransferTransaction> TransferMoney(MoneyTransferTransaction transaction)
    {
        var sourceAccount = transaction.SourceAccount;
        var destinationAccount = transaction.DestinationAccount;

        var sourceBalance = sourceAccount.Balance - transaction.Amount;
        if (sourceBalance < 0m)
        {
            return new TransactionResult<MoneyTransferTransaction>(transaction, "Insufficient funds to make transfer");
        }

        if (sourceBalance < Account.LowFundsThreshold)
        {
            notificationService.NotifyFundsLow(sourceAccount.User.Email);
        }

        var paidIn = destinationAccount.PaidIn + transaction.Amount;
        if (paidIn > Account.PayInLimit)
        {
            return new TransactionResult<MoneyTransferTransaction>(transaction, "Account pay in limit reached");
        }

        if (Account.PayInLimit - paidIn < Account.PayInWarningThreshold)
        {
            notificationService.NotifyApproachingPayInLimit(destinationAccount.User.Email);
        }

        sourceAccount.Balance -= transaction.Amount;
        sourceAccount.Withdrawn -= transaction.Amount;

        destinationAccount.Balance += transaction.Amount;
        destinationAccount.PaidIn += transaction.Amount;

        return new TransactionResult<MoneyTransferTransaction>(transaction);
    }
}
