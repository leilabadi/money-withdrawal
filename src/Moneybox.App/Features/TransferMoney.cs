using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Features
{
    public class TransferMoney(IMoneyTransferService moneyTransferService, IAccountRepository accountRepository)
    {
        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            var transaction = new MoneyTransferTransaction(amount, DateTime.UtcNow, from, to);

            var result = moneyTransferService.TransferMoney(transaction);

            accountRepository.Update(result.Transaction.SourceAccount);
            accountRepository.Update(result.Transaction.DestinationAccount);
        }
    }
}
