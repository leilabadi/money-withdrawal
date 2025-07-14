namespace Moneybox.App.Domain.Events;

public record ApproachingPayInLimitEvent(Guid AccountId, string Email);