using Moneybox.App.Application;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;
using Moneybox.App.Tests.Common;

namespace Moneybox.App.Tests.Application;

public class WithdrawMoneyTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly WithdrawMoney _withdrawMoney;
    private readonly Account _sourceAccount;

    public WithdrawMoneyTests()
    {
        _sourceAccount = _fixture.Build<Account>()
            .With(p => p.Balance, 0)
            .With(p => p.Withdrawn, 0)
            .With(p => p.PaidIn, 0)
            .Create();

        _accountRepositoryMock.Setup(x => x.GetAccountById(_sourceAccount.Id)).Returns(_sourceAccount);

        _withdrawMoney = new WithdrawMoney(_accountRepositoryMock.Object, _notificationServiceMock.Object);
    }

    [Fact]
    public void Execute_WhenWithdrawingMoney_AccountsShouldBeUpdated()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        decimal withdrawAmount = 300;

        // Act
        _withdrawMoney.Execute(_sourceAccount.Id, withdrawAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 700, -withdrawAmount, 0))), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountIsLowFunds_ShouldNotifySender()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        decimal withdrawAmount = 501;

        // Act
        _withdrawMoney.Execute(_sourceAccount.Id, withdrawAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -withdrawAmount, 0))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyFundsLow(_sourceAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountHasInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        _sourceAccount.Balance = 100;
        decimal withdrawAmount = 200;

        // Act
        Action action = () => _withdrawMoney.Execute(_sourceAccount.Id, withdrawAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds to make transfer");
    }
}
