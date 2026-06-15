using Festpay.Onboarding.Application.Features.V1.Transaction;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Domain.Exceptions;
using Festpay.Onboarding.Infra.Context;
using Microsoft.EntityFrameworkCore;

namespace Festpay.Onboarding.Application.Tests.Features.V1.Transaction;

public class CreateTransactionCommandHandlerTests
{
    private readonly DbContextOptions<FestpayContext> _dbOptions;

    public CreateTransactionCommandHandlerTests()
    {
        _dbOptions = new DbContextOptionsBuilder<FestpayContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task Should_Create_Transaction_Successfully()
    {
        // Arrange
        using var context = new FestpayContext(_dbOptions);
        
        var sourceAccount = new Account.Builder()
            .WithName("Source")
            .WithDocument("34180123029")
            .WithEmail("s@test.com")
            .WithPhone("11999999999")
            .Build();
            
        sourceAccount.Deposit(300.00m);
            
        var destAccount = new Account.Builder()
            .WithName("Dest")
            .WithDocument("46994242013")
            .WithEmail("d@test.com")
            .WithPhone("11999999999")
            .Build();
        
        context.Accounts.Add(sourceAccount);
        context.Accounts.Add(destAccount);
        await context.SaveChangesAsync();

        var command = new CreateTransactionCommand(sourceAccount.Id, destAccount.Id, 250.00m);
        var handler = new CreateTransactionCommandHandler(context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        var transactionsInDb = await context.Transactions.ToListAsync();

        // Assert
        Assert.True(result);
        Assert.Single(transactionsInDb);
        Assert.Equal(250.00m, transactionsInDb.First().Amount);
        
        Assert.Equal(50.00m, sourceAccount.Balance);
        Assert.Equal(250.00m, destAccount.Balance);
    }
    
    [Fact]
    public async Task Should_Throw_InvalidOperationException_When_Insufficient_Balance()
    {
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
        
        context.Accounts.Add(sourceAccount);
        context.Accounts.Add(destAccount);
        await context.SaveChangesAsync();

        var command = new CreateTransactionCommand(sourceAccount.Id, destAccount.Id, 250.00m);
        var handler = new CreateTransactionCommandHandler(context);

        await Assert.ThrowsAsync<InsufficientBalanceException>(
            () => handler.Handle(command, CancellationToken.None)
        );
    }
}