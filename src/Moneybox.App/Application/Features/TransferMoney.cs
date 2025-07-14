using Moneybox.App.Domain.Events;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Application.Features
{
    public class TransferMoney(IMoneyTransferService moneyTransferService, IAccountRepository accountRepository, INotificationService notificationService, ITransaction transaction)
    {
        public Result Execute(Guid idempotencyKey, Guid fromAccountId, Guid toAccountId, decimal amount)
        {
            // Check idempotency key to prevent duplicate transactions

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            }
            if (fromAccountId == toAccountId)
            {
                throw new ArgumentException("Source and destination accounts must be different.");
            }

            var from = accountRepository.GetAccountById(fromAccountId);
            var to = accountRepository.GetAccountById(toAccountId);

            if (from == null)
            {
                throw new InvalidOperationException("Source account not found.");
            }
            if (to == null)
            {
                throw new InvalidOperationException("Destination account not found.");
            }

            var paymentTransaction = TransactionFactory.CreateMoneyTransferTransaction(amount, from, to);

            TransactionResult<MoneyTransferTransaction> result;

            try
            {
                transaction.Begin();

                result = moneyTransferService.TransferMoney(paymentTransaction);

                if (!result.IsSuccessful)
                {
                    transaction.Rollback();
                    return Result.Failure(result.ErrorMessage!);
                }

                // Use optimistic concurrency control like row versioning in a real application to avoid conflicting transactions
                accountRepository.Update(result.Transaction.SourceAccount);
                accountRepository.Update(result.Transaction.DestinationAccount);

                transaction.Commit();
            }
            catch (Exception)
            {
                // Log the exception
                transaction.Rollback();
                throw;
            }

            // Use outbox pattern to handle notifications
            foreach (var domainEvent in result.Transaction.DomainEvents)
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

            return Result.Success();
        }
    }
}
