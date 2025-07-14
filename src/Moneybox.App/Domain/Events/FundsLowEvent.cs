namespace Moneybox.App.Domain.Events;

public record FundsLowEvent(Guid AccountId, string Email);