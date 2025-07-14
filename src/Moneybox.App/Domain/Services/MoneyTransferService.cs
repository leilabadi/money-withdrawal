using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public class MoneyTransferService : IMoneyTransferService
{
    public TransactionResult<MoneyTransferTransaction> TransferMoney(MoneyTransferTransaction transaction)
    {
        var result = transaction.Validate();
        if (!result.IsValid)
        {
            return TransactionResult<MoneyTransferTransaction>.Failure(transaction, result.ErrorMessage!);
        }

        var sourceAccount = transaction.SourceAccount;
        var destinationAccount = transaction.DestinationAccount;

        // In real applications, should add ledger entries to both accounts and create audit logs
        sourceAccount.Balance -= transaction.Amount;
        sourceAccount.Withdrawn -= transaction.Amount;

        destinationAccount.Balance += transaction.Amount;
        destinationAccount.PaidIn += transaction.Amount;

        return TransactionResult<MoneyTransferTransaction>.Success(transaction);
    }
}
