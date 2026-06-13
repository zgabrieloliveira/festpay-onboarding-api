using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Domain.Exceptions;

namespace Festpay.Onboarding.Domain.Tests.Entities;

public class TransactionTests
{
    [Fact]
    public void Should_Create_Transaction_When_Data_Is_Valid()
    {
        var sourceId = Guid.NewGuid();
        var destId = Guid.NewGuid();

        var transaction = new Transaction.Builder()
            .WithSourceAccount(sourceId)
            .WithDestinationAccount(destId)
            .WithAmount(100.50m)
            .Build();

        Assert.Equal(sourceId, transaction.SourceAccountId);
        Assert.Equal(destId, transaction.DestinationAccountId);
        Assert.Equal(100.50m, transaction.Amount);
        Assert.False(transaction.IsCancelled);
    }

    [Fact]
    public void Should_Throw_RequiredFieldException_When_SourceAccount_Is_Empty()
    {
        var exception = Assert.Throws<RequiredFieldException>(
            () => new Transaction.Builder()
                    .WithSourceAccount(Guid.Empty)
                    .WithDestinationAccount(Guid.NewGuid())
                    .WithAmount(100)
                    .Build()
        );
        Assert.Equal("SourceAccountId", exception.FieldName);
    }

    [Fact]
    public void Should_Throw_RequiredFieldException_When_DestinationAccount_Is_Empty()
    {
        var exception = Assert.Throws<RequiredFieldException>(
            () => new Transaction.Builder()
                    .WithSourceAccount(Guid.NewGuid())
                    .WithDestinationAccount(Guid.Empty)
                    .WithAmount(100)
                    .Build()
        );
        Assert.Equal("DestinationAccountId", exception.FieldName);
    }

    [Fact]
    public void Should_Throw_SameSourceAndDestinationAccountException_When_Accounts_Are_Equal()
    {
        var accountId = Guid.NewGuid();

        Assert.Throws<SameSourceAndDestinationAccountException>(
            () => new Transaction.Builder()
                    .WithSourceAccount(accountId)
                    .WithDestinationAccount(accountId)
                    .WithAmount(100)
                    .Build()
        );
    }

    [Fact]
    public void Should_Throw_InvalidTransactionAmountException_When_Amount_Is_Zero_Or_Negative()
    {
        var exception = Assert.Throws<InvalidTransactionAmountException>(
            () => new Transaction.Builder()
                    .WithSourceAccount(Guid.NewGuid())
                    .WithDestinationAccount(Guid.NewGuid())
                    .WithAmount(0)
                    .Build()
        );
        Assert.Equal(0, exception.Amount);
    }

    [Fact]
    public void Should_Cancel_Transaction_When_Not_Cancelled()
    {
        var transaction = new Transaction.Builder()
            .WithSourceAccount(Guid.NewGuid())
            .WithDestinationAccount(Guid.NewGuid())
            .WithAmount(100)
            .Build();

        transaction.Cancel();

        Assert.True(transaction.IsCancelled);
    }

    [Fact]
    public void Should_Throw_TransactionAlreadyCancelledException_When_Already_Cancelled()
    {
        var transaction = new Transaction.Builder()
            .WithSourceAccount(Guid.NewGuid())
            .WithDestinationAccount(Guid.NewGuid())
            .WithAmount(100)
            .Build();

        transaction.Cancel(); // First cancellation

        Assert.Throws<TransactionAlreadyCancelledException>(() => transaction.Cancel()); // Second throws
    }
}