namespace Moneybox.App.Domain.Model;

public class User(Guid id, string name, string email)
{
    public Guid Id { get; init; } = id;
    public string Name { get; init; } = name;
    public string Email { get; init; } = email;
}
