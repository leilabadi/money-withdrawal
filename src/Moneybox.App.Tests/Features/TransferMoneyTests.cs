using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;
using Moneybox.App.Features;
using Moneybox.App.Tests.Common;

namespace Moneybox.App.Tests.Features;

public class TransferMoneyTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly TransferMoney _transferMoney;
    private readonly Account _sourceAccount;
    private readonly Account _destinationAccount;

    public TransferMoneyTests()
    {
        _sourceAccount = _fixture.Build<Account>()
            .With(p => p.Balance, 0)
            .With(p => p.Withdrawn, 0)
            .With(p => p.PaidIn, 0)
            .Create();

        _destinationAccount = _fixture.Build<Account>()
            .With(p => p.Balance, 0)
            .With(p => p.Withdrawn, 0)
            .With(p => p.PaidIn, 0)
            .Create();

        _accountRepositoryMock.Setup(x => x.GetAccountById(_sourceAccount.Id)).Returns(_sourceAccount);
        _accountRepositoryMock.Setup(x => x.GetAccountById(_destinationAccount.Id)).Returns(_destinationAccount);

        _moneyTransferService = new MoneyTransferService(_notificationServiceMock.Object);

        _transferMoney = new TransferMoney(_moneyTransferService, _accountRepositoryMock.Object);
    }

    [Fact]
    public void Execute_WhenTransferingMoney_AccountsShouldBeUpdated()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        _destinationAccount.Balance = 500;
        decimal transferAmount = 300;

        // Act
        _transferMoney.Execute(_sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 700, -transferAmount, 0))), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_destinationAccount.Id, 800, 0, transferAmount))), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountIsLowFunds_ShouldNotifySender()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 501;

        // Act
        _transferMoney.Execute(_sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -transferAmount, 0))), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_destinationAccount.Id, transferAmount, 0, transferAmount))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyFundsLow(_sourceAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountHasInsufficientFunds_ShouldThrowException()
    {
        // Arrange
        _sourceAccount.Balance = 100;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 200;

        // Act
        Action action = () => _transferMoney.Execute(_sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        action.Should().Throw<InvalidOperationException>().WithMessage("Insufficient funds to make transfer");
    }

    [Fact]
    public void Execute_WhenDestinationAccountApproachingPayInLimit_ShouldNotifyReceiver()
    {
        // Arrange
        _sourceAccount.Balance = 4000;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 3501;

        // Act
        _transferMoney.Execute(_sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -transferAmount, 0))), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_destinationAccount.Id, transferAmount, 0, transferAmount))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyApproachingPayInLimit(_destinationAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenDestinationAccountReachedPayInLimit_ShouldThrowException()
    {
        // Arrange
        _sourceAccount.Balance = 5000;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 4001;

        // Act
        Action action = () => _transferMoney.Execute(_sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        action.Should().Throw<InvalidOperationException>().WithMessage("Account pay in limit reached");
    }
}