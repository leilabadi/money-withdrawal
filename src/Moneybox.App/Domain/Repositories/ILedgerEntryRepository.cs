using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Repositories;

public interface ILedgerEntryRepository
{
    IList<LedgerEntry> GetLedgerEntriesByAccountId(Guid accountId);
    void AddLedgerEntry(LedgerEntry transaction);
}
