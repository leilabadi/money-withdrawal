using Moneybox.App.Domain.Events;

namespace Moneybox.App.Domain.Model;

public class MoneyWithdrawalTransaction : Transaction
{
    public Account SourceAccount { get; }

    public MoneyWithdrawalTransaction(decimal amount, DateTime date, Account sourceAccount)
        : base(amount, date)
    {
        SourceAccount = sourceAccount;
    }

    public override ValidationResult Validate()
    {
        // Basic validation checks
        if (Amount <= 0)
        {
            return ValidationResult.Failure("Withdrawal amount must be greater than zero");
        }

        // Check if source account has sufficient funds
        if (SourceAccount.Balance < Amount)
        {
            return ValidationResult.Failure("Insufficient funds to make withdrawal");
        }

        CheckWarningConditions();

        return ValidationResult.Success();
    }

    private void CheckWarningConditions()
    {
        if (SourceAccount.Balance - Amount < Account.LowFundsThreshold)
        {
            DomainEvents.Add(new FundsLowEvent(SourceAccount.Id, SourceAccount.User.Email));
        }
    }
}
