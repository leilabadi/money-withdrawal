using Moneybox.App.Domain.Model;

namespace Moneybox.App.Domain.Services;

public interface ILedgerService
{
    IList<LedgerEntry> GetAccountEntries(Guid accountId);
    void RecordEntry(LedgerEntry entry);
}
