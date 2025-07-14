using Moneybox.App.Application;
using Moneybox.App.Application.Features;
using Moneybox.App.Domain.Model;
using Moneybox.App.Domain.Repositories;
using Moneybox.App.Domain.Services;
using Moneybox.App.Tests.Common;

namespace Moneybox.App.Tests.Features;

public class WithdrawMoneyTests
{
    private readonly Fixture _fixture = new();
    private readonly Mock<IAccountRepository> _accountRepositoryMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ITransaction> _transactionMock = new();
    private readonly IMoneyWithdrawalService _moneyWithdrawalService;
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

        _moneyWithdrawalService = new MoneyWithdrawalService();

        _withdrawMoney = new WithdrawMoney(_moneyWithdrawalService, _accountRepositoryMock.Object, _notificationServiceMock.Object, _transactionMock.Object);
    }

    [Fact]
    public void Execute_WhenWithdrawingMoney_AccountsShouldBeUpdated()
    {
        // Arrange
        _sourceAccount.Balance = 1000;
        decimal withdrawAmount = 300;

        // Act
        _withdrawMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, withdrawAmount);

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
        _withdrawMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, withdrawAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Once);
        _accountRepositoryMock.Verify(x => x.Update(It.Is<Account>(a => a.Matches(_sourceAccount.Id, 499, -withdrawAmount, 0))), Times.Once);

        _notificationServiceMock.Verify(x => x.NotifyFundsLow(_sourceAccount.User.Email), Times.Once);
    }

    [Fact]
    public void Execute_WhenSourceAccountHasInsufficientFunds_ShouldReturnFailure()
    {
        // Arrange
        _sourceAccount.Balance = 100;
        decimal withdrawAmount = 200;

        // Act
        var result = _withdrawMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, withdrawAmount);

        // Assert
        _accountRepositoryMock.Verify(x => x.Update(It.IsAny<Account>()), Times.Never);

        result.IsSuccessful.Should().BeFalse();
        result.ErrorMessage.Should().Be("Insufficient funds to make withdrawal");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Execute_WhenAmountIsNotPositive_ShouldThrowArgumentException(decimal invalidAmount)
    {
        // Act
        var ex = Record.Exception(() =>
            _withdrawMoney.Execute(Guid.NewGuid(), _sourceAccount.Id, invalidAmount)
        );

        // Assert
        ex.Should().BeOfType<ArgumentException>()
            .Which.Message.Should().Contain("Amount must be positive");
    }

    [Fact]
    public void Execute_WhenSourceAccountDoesNotExist_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var nonExistentSourceId = Guid.NewGuid();
        _accountRepositoryMock.Setup(x => x.GetAccountById(nonExistentSourceId)).Returns((Account?)null);

        // Act
        var ex = Record.Exception(() =>
            _withdrawMoney.Execute(Guid.NewGuid(), nonExistentSourceId, 100)
        );

        // Assert
        ex.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain("Source account not found");
    }
}
