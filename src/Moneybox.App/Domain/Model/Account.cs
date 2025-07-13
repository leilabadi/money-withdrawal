namespace Moneybox.App.Domain.Model;

public class Account(Guid id, User user, decimal balance, decimal withdrawn, decimal paidIn)
{
    public const decimal PayInLimit = 4000m;
    public const decimal PayInWarningThreshold = 500m;
    public const decimal LowFundsThreshold = 500m;

    public Guid Id { get; init; } = id;
    public User User { get; init; } = user;
    public decimal Balance { get; set; } = balance;
    public decimal Withdrawn { get; set; } = withdrawn;
    public decimal PaidIn { get; set; } = paidIn;
}
