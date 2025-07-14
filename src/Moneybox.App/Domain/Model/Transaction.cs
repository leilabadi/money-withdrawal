namespace Moneybox.App.Domain.Model;

public abstract class Transaction(decimal amount, DateTime date)
{
    public Guid Id { get; } = Guid.NewGuid();
    public decimal Amount { get; } = amount;
    public DateTime Date { get; } = date;
    public List<object> DomainEvents { get; } = [];

    public abstract ValidationResult Validate();
}
