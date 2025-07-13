namespace Moneybox.App.Tests.Common;

internal static class TestAccountExtension
{
    public static bool Matches(this Account account, Guid accountId, decimal balance, decimal withdrawn, decimal paidIn)
    {
        return account.Id == accountId
            && account.Balance == balance
            && account.Withdrawn == withdrawn
            && account.PaidIn == paidIn;
    }
}
