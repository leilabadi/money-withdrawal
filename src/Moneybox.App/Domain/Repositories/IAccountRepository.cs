using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Repositories;

public interface IAccountRepository
{
    Account GetAccountById(Guid accountId);
    void Update(Account account);
}
