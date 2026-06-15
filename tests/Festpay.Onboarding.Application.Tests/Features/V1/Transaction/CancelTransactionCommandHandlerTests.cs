using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class CancelTransactionCommandHandlerTests
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public CancelTransactionCommandHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private static Domain.Entities.Transaction CreateTestTransaction(Guid sourceId, Guid destId)
    {
        return new Domain.Entities.Transaction.Builder()
            .WithSourceAccount(sourceId)
            .WithDestinationAccount(destId)
            .WithAmount(150.00m)
            .Build();
    }

    [Fact]
    public async Task Should_Cancel_Transaction_When_Exists_And_Not_Cancelled()
    {
        // Arrange
        using var context = new FestpayContext(_dbOptions);
        
        var sourceAccount = new Account.Builder()
            .WithName("Source")
            .WithDocument("34180123029")
            .WithEmail("source@test.com")
            .WithPhone("11999999999")
            .Build();

        var destAccount = new Account.Builder()
            .WithName("Dest")
            .WithDocument("46994242013")
            .WithEmail("dest@test.com")
            .WithPhone("11999999999")
            .Build();
        
        destAccount.Deposit(150.00m);

        context.Accounts.Add(sourceAccount);
        context.Accounts.Add(destAccount);

        var transaction = CreateTestTransaction(sourceAccount.Id, destAccount.Id);
        context.Transactions.Add(transaction);
        await context.SaveChangesAsync();

        var command = new CancelTransactionCommand(transaction.Id);
        var handler = new CancelTransactionCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var updatedTransaction = await context.Transactions.FindAsync(transaction.Id);

        // Assert
        Assert.True(result);
        Assert.True(updatedTransaction!.IsCancelled);
        
        Assert.Equal(150.00m, sourceAccount.Balance);
        Assert.Equal(0.00m, destAccount.Balance);
    }

    [Fact]
    public async Task Should_Throw_NotFoundException_When_Transaction_Not_Exists()
    {
        using var context = new FestpayContext(_dbOptions);
        var command = new CancelTransactionCommand(Guid.NewGuid());
        var handler = new CancelTransactionCommandHandler(context);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }
}