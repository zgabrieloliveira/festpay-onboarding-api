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

    private static Domain.Entities.Transaction CreateTestTransaction()
    {
        return new Domain.Entities.Transaction.Builder()
            .WithSourceAccount(Guid.NewGuid())
            .WithDestinationAccount(Guid.NewGuid())
            .WithAmount(150.00m)
            .Build();
    }

    [Fact]
    public async Task Should_Cancel_Transaction_When_Exists_And_Not_Cancelled()
    {
        // Arrange
        var transaction = CreateTestTransaction();

        using var context = new FestpayContext(_dbOptions);
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