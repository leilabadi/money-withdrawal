using Moneybox.App.Domain.Events;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Features
{
    public class TransferMoney(IMoneyTransferService moneyTransferService, IAccountRepository accountRepository, INotificationService notificationService)
    {
        public void Execute(Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            var transaction = TransactionFactory.CreateMoneyTransferTransaction(amount, from, to);

            var result = moneyTransferService.TransferMoney(transaction);

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
                    case ApproachingPayInLimitEvent e:
                        notificationService.NotifyApproachingPayInLimit(e.Email);
                        break;
                }
            }

            accountRepository.Update(result.Transaction.SourceAccount);
            accountRepository.Update(result.Transaction.DestinationAccount);
        }
    }
}
