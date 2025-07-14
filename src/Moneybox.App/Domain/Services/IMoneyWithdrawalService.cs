using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public interface IMoneyWithdrawalService
{
    TransactionResult<MoneyWithdrawalTransaction> WithdrawMoney(MoneyWithdrawalTransaction transaction);
}
