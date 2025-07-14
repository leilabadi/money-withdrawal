using Moneybox.App.Application;
using Moneybox.App.Application.Features;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;
using Moneybox.App.Tests.Common;

namespace Moneybox.App.Tests.Features;

public class TransferMoneyTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ITransaction> _transactionMock = new();
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

        _moneyTransferService = new MoneyTransferService();

        _transferMoney = new TransferMoney(_moneyTransferService, _accountRepositoryMock.Object, _notificationServiceMock.Object, _transactionMock.Object);
    }

    [Fact]
    public void Execute_WhenTransferingMoney_AccountsShouldBeUpdated()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        _destinationAccount.Balance = 500;
        decimal transferAmount = 300;

        // Act
        _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, transferAmount);

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
        _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -transferAmount, 0))), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_destinationAccount.Id, transferAmount, 0, transferAmount))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyFundsLow(_sourceAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountHasInsufficientFunds_ShouldReturnFailure()
    {
        // Arrange
        _sourceAccount.Balance = 100;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 200;

        // Act
        var result = _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().Be("Insufficient funds to make transfer");
    }

    [Fact]
    public void Execute_WhenDestinationAccountApproachingPayInLimit_ShouldNotifyReceiver()
    {
        // Arrange
        _sourceAccount.Balance = 4000;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 3501;

        // Act
        _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Exactly(2));
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -transferAmount, 0))), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_destinationAccount.Id, transferAmount, 0, transferAmount))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyApproachingPayInLimit(_destinationAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenDestinationAccountReachedPayInLimit_ShouldReturnFailure()
    {
        // Arrange
        _sourceAccount.Balance = 5000;
        _destinationAccount.Balance = 0;
        decimal transferAmount = 4001;

        // Act
        var result = _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, transferAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().Be("Account pay in limit reached");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Execute_WhenAmountIsNotPositive_ShouldThrowArgumentException(decimal invalidAmount)
    {
        // Act
        var ex = Record.Exception(() =>
            _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, _destinationAccount.Id, invalidAmount)
        );

        // Assert
        ex.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Amount must be positive");
    }

    [Fact]
    public void Execute_WhenSourceAndDestinationAccountsAreTheSame_ShouldThrowArgumentException()
    {
        // Arrange
        var sameAccountId = _sourceAccount.Id;
        decimal transferAmount = 100;

        // Act
        var ex = Record.Exception(() =>
            _transferMoney.Execute(Guid.NewGuid(), sameAccountId, sameAccountId, transferAmount)
        );

        // Assert
        ex.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Source and destination accounts must be different");
    }

    [Fact]
    public void Execute_WhenSourceAccountDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentSourceId = Guid.NewGuid();
        _accountRepositoryMock.Setup(x => x.GetAccountById(nonExistentSourceId)).Returns((Account?)null);

        // Act
        var ex = Record.Exception(() =>
            _transferMoney.Execute(Guid.NewGuid(), nonExistentSourceId, _destinationAccount.Id, 100)
        );

        // Assert
        ex.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain("Source account not found");
    }

    [Fact]
    public void Execute_WhenDestinationAccountDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentDestinationId = Guid.NewGuid();
        _accountRepositoryMock.Setup(x => x.GetAccountById(nonExistentDestinationId)).Returns((Account?)null);

        // Act
        var ex = Record.Exception(() =>
            _transferMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, nonExistentDestinationId, 100)
        );

        // Assert
        ex.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain("Destination account not found");
    }
}