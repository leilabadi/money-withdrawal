using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;

namespace Moneybox.App.Domain.Services;

public class LedgerService(ILedgerEntryRepository repository) : ILedgerService
{
    public IList<LedgerEntry> GetAccountEntries(Guid accountId)
    {
        return repository.GetLedgerEntriesByAccountId(accountId);
    }

    public void RecordEntry(LedgerEntry entry)
    {
        repository.AddLedgerEntry(entry);
    }
}
