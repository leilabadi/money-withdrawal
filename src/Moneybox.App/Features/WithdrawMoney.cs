using Moneybox.App.DataAccess;
using Moneybox.App.Domain.Services;
using System;

namespace Moneybox.App.Features
{
    public class WithdrawMoney(IAccountRepository accountRepository, INotificationService notificationService)
    {
        public void Execute(Guid fromAccountId, decimal amount)
        {
            var from = accountRepository.GetAccountById(fromAccountId);

            var fromBalance = from.Balance - amount;
            if (fromBalance < 0m)
            {
                throw new InvalidOperationException("Insufficient funds to make transfer");
            }

            if (fromBalance < 500m)
            {
                notificationService.NotifyFundsLow(from.User.Email);
            }

            from.Balance = from.Balance - amount;
            from.Withdrawn = from.Withdrawn - amount;

            accountRepository.Update(from);
        }
    }
}
