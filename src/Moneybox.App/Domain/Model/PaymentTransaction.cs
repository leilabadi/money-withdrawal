namespace Moneybox.App.Domain.Model;

public abstract class PaymentTransaction(decimal amount, DateTime date)
{
    public Guid Id { get; } = Guid.NewGuid();

    // In real world applications, we should handle currencies properly
    public decimal Amount { get; } = amount;
    
    public DateTime Date { get; } = date;
    public List<object> DomainEvents { get; } = [];

    public abstract ValidationResult Validate();
}
