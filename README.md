# Moneybox Money Withdrawal

The solution contains a .NET core library (Moneybox.App) which is structured into the following 3 folders:

* Domain - this contains the domain models for a user and an account, and a notification service.
* Features - this contains two operations, one which is implemented (transfer money) and another which isn't (withdraw money)
* DataAccess - this contains a repository for retrieving and saving an account (and the nested user it belongs to)

## The task

The task is to implement a money withdrawal in the WithdrawMoney.Execute(...) method in the features folder. For consistency, the logic should be the same as the TransferMoney.Execute(...) method i.e. notifications for low funds and exceptions where the operation is not possible. 

As part of this process however, you should look to refactor some of the code in the TransferMoney.Execute(...) method into the domain models, and make these models less susceptible to misuse. We're looking to make our domain models rich in behaviour and much more than just plain old objects, however we don't want any data persistance operations (i.e. data access repositories) to bleed into our domain. This should simplify the task of implementing WithdrawMoney.Execute(...).

## Guidelines

* The test should take about an hour to complete, although there is no strict time limit
* You should fork or copy this repository into your own public repository (Github, BitBucket etc.) before you do your work
* Your solution must build and any tests must pass
* You should not alter the notification service or the the account repository interfaces
* You may add unit/integration tests using a test framework (and/or mocking framework) of your choice
* You may edit this README.md if you want to give more details around your work (e.g. why you have done something a particular way, or anything else you would look to do but didn't have time)

Once you have completed test, zip up your solution, excluding any build artifacts to reduce the size, and email it back to our recruitment team.

Good luck!

## Design Decisions

### 1. Domain-Driven Design

- **Separation of Concerns:** The domain logic is encapsulated in model classes (`Account`, `MoneyTransferTransaction`, `MoneyWithdrawalTransaction`) and service interfaces (`IMoneyTransferService`, `IMoneyWithdrawalService`). This separation allows for clear boundaries between business logic and application orchestration.
- **Validation Logic:** Each transaction type (`MoneyTransferTransaction`, `MoneyWithdrawalTransaction`) contains its own `Validate()` method, ensuring that all business rules are enforced before any state changes occur.
- **Event-Driven Warnings:** Domain events (e.g., `FundsLowEvent`, `ApproachingPayInLimitEvent`) are raised within validation methods to decouple warning/notification logic from core business logic.
- **Domain Services** `MoneyTransferService` & `MoneyWithdrawalService` encapsulate the logic for updating account balances and tracking withdrawals/paid-in amounts. They return a `TransactionResult<T>` to indicate success or failure, along with error messages when appropriate.
- **TransactionFactory:** Provides static methods to create transaction objects, centralizing the instantiation logic and ensuring consistent use of timestamps.

### 2. Error Handling

- **Business Validation:** I changed the code to checks for business rule violations and returns appropriate error messages rather than throwing exceptions.
- **General Validation:** To ensuring that invalid data does not reach the domain layer, input validation is performed in application services throwing exceptions when necessary.
- **Exception Handling:** Exceptions are caught in the application layer, to allow proper transaction handling and rollback if needed.

### 3. Testability

- **Mocking Dependencies:** Tests use Moq to mock repositories and notification services, allowing for isolated and deterministic unit tests.
- **AutoFixture for Test Data:** AutoFixture is used to generate test data, reducing boilerplate and ensuring test coverage across a range of scenarios.
- **FluentAssertions:** Used for expressive and readable assertions in tests.

## Suggestions for Future Improvements

While the current implementation updates account balances, but a real world system would require the following features:

- Using Outbox pattern to ensure reliable message delivery for notifications
- Recording Ledger entries for each transaction
- Adding Audit logs to track all changes and access
- Potentially calculating and charging fees on transaction
- Handling different currencies