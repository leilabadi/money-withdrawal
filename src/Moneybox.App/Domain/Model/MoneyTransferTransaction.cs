using Moneybox.App.Domain.Events;

namespace Moneybox.App.Domain.Model;

public class MoneyTransferTransaction : Transaction
{
    public Account SourceAccount { get; }
    public Account DestinationAccount { get; }

    public MoneyTransferTransaction(decimal amount, DateTime date, Account sourceAccount, Account destinationAccount)
        : base(amount, date)
    {
        SourceAccount = sourceAccount;
        DestinationAccount = destinationAccount;
    }

    public override ValidationResult Validate()
    {
        // Basic validation checks
        if (Amount <= 0)
        {
            return ValidationResult.Failure("Transfer amount must be greater than zero");
        }
        if (SourceAccount == DestinationAccount)
        {
            return ValidationResult.Failure("Source and destination accounts cannot be the same");
        }

        // Check if source account has sufficient funds
        if (SourceAccount.Balance < Amount)
        {
            return ValidationResult.Failure("Insufficient funds to make transfer");
        }

        // Check pay-in limits for destination account
        var paidIn = DestinationAccount.PaidIn + Amount;
        if (paidIn > Account.PayInLimit)
        {
            return ValidationResult.Failure("Account pay in limit reached");
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

        var paidIn = DestinationAccount.PaidIn + Amount;
        if (Account.PayInLimit - paidIn < Account.PayInWarningThreshold)
        {
            DomainEvents.Add(new ApproachingPayInLimitEvent(DestinationAccount.Id, DestinationAccount.User.Email));
        }
    }
}
