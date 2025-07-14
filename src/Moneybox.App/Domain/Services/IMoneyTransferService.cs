using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public interface IMoneyTransferService
{
    TransactionResult<MoneyTransferTransaction> TransferMoney(MoneyTransferTransaction transaction);
}
