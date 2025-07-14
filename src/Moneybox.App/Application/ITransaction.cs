namespace Moneybox.App.Application;

public interface ITransaction
{
    void Begin();
    void Commit();
    void Rollback();
}