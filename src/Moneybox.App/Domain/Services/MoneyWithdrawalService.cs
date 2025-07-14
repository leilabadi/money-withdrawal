using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public class MoneyWithdrawalService : IMoneyWithdrawalService
{
    public TransactionResult<MoneyWithdrawalTransaction> WithdrawMoney(MoneyWithdrawalTransaction transaction)
    {
        var result = transaction.Validate();
        if (!result.IsValid)
        {
            return TransactionResult<MoneyWithdrawalTransaction>.Failure(transaction, result.ErrorMessage!);
        }

        var sourceAccount = transaction.SourceAccount;

        // In real applications, should add ledger entries to the account and create audit logs
        sourceAccount.Balance -= transaction.Amount;
        sourceAccount.Withdrawn -= transaction.Amount;

        return TransactionResult<MoneyWithdrawalTransaction>.Success(transaction);
    }
}
