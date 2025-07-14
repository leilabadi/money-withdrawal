using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Features
{
    public class WithdrawMoney(IMoneyWithdrawalService moneyWithdrawalService, IAccountRepository accountRepository)
    {
        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);

            var transaction = new MoneyWithdrawalTransaction(amount, DateTime.UtcNow, from);

            var result = moneyWithdrawalService.WithdrawMoney(transaction);

            if (!result.IsSuccessful)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            accountRepository.Update(result.Transaction.SourceAccount);
        }
    }
}
