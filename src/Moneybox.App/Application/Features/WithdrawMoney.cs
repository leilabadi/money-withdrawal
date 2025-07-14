using Moneybox.App.Domain.Events;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;

namespace Moneybox.App.Application.Features
{
    public class WithdrawMoney(IMoneyWithdrawalService moneyWithdrawalService, IAccountRepository accountRepository, INotificationService notificationService, ITransaction transaction)
    {
        public Result Execute(Guid idempotencyKey, Guid fromAccountId, decimal amount)
        {
            // Check idempotency key to prevent duplicate transactions

            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be positive.", nameof(amount));
            }

            var from = accountRepository.GetAccountById(fromAccountId);

            if (from == null)
            {
                throw new InvalidOperationException("Source account not found.");
            }

            var paymentTransaction = TransactionFactory.CreateMoneyWithdrawalTransaction(amount, from);

            TransactionResult<MoneyWithdrawalTransaction> result;

            try
            {
                transaction.Begin();

                result = moneyWithdrawalService.WithdrawMoney(paymentTransaction);

                if (!result.IsSuccessful)
                {
                    transaction.Rollback();
                    return Result.Failure(result.ErrorMessage!);
                }

                // Use optimistic concurrency control like row versioning in a real application to avoid conflicting transactions
                accountRepository.Update(result.Transaction.SourceAccount);

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
                }
            }

            return Result.Success();
        }
    }
}
