using Moneybox.App.Domain.Events;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Features
{
    public class WithdrawMoney(IMoneyWithdrawalService moneyWithdrawalService, IAccountRepository accountRepository, INotificationService notificationService)
    {
        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);

            var transaction = TransactionFactory.CreateMoneyWithdrawalTransaction(amount, from);

            var result = moneyWithdrawalService.WithdrawMoney(transaction);

            if (!result.IsSuccessful)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            foreach (var domainEvent in transaction.DomainEvents)
            {
                switch (domainEvent)
                {
                    case FundsLowEvent e:
                        notificationService.NotifyFundsLow(e.Email);
                        break;
                }
            }

            accountRepository.Update(result.Transaction.SourceAccount);
        }
    }
}
