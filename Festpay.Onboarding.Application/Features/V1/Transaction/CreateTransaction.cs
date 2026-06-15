using FluentValidation;
using MediatR;
using Festpay.Onboarding.Infra.Context;
using Festpay.Onboarding.Domain.Entities;
using Carter;
using Microsoft.AspNetCore.Routing;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Festpay.Onboarding.Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record CreateTransactionCommand(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount
) : IRequest<bool>;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.SourceAccountId)
            .NotEmpty()
            .WithMessage("Source account is required.");

        RuleFor(x => x.DestinationAccountId)
            .NotEmpty()
            .WithMessage("Destination account is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Transaction amount must be greater than zero.");
    }
}

public sealed class CreateTransactionCommandHandler(FestpayContext dbContext) 
    : IRequestHandler<CreateTransactionCommand, bool>
{
    public async Task<bool> Handle(CreateTransactionCommand request, CancellationToken cancellationToken)
    {
        var sourceAccount = await dbContext.Accounts.FindAsync([request.SourceAccountId], cancellationToken)
            ?? throw new NotFoundException("Source Account");

        var destinationAccount = await dbContext.Accounts.FindAsync([request.DestinationAccountId], cancellationToken)
            ?? throw new NotFoundException("Destination Account");

        sourceAccount.Withdraw(request.Amount);
        destinationAccount.Deposit(request.Amount);

        var transaction = new Domain.Entities.Transaction.Builder()
            .WithSourceAccount(request.SourceAccountId)
            .WithDestinationAccount(request.DestinationAccountId)
            .WithAmount(request.Amount)
            .Build();

        await dbContext.Transactions.AddAsync(transaction, cancellationToken);
        
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class CreateTransactionEndpoint : ICarterModule
{
    /// <summary>
    /// Creates a new financial transaction between two accounts.
    /// </summary>
    /// <param name="command">The transaction details (SourceAccountId, DestinationAccountId, Amount).</param>
    /// <response code="201">Returns the newly created transaction.</response>
    /// <response code="400">If the input data is invalid or the source account has insufficient balance.</response>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConstants.V1}{EndpointConstants.Transaction}",
            async ([FromServices] ISender sender, [FromBody] CreateTransactionCommand command) =>
            {
                var result = await sender.Send(command);
                return Result.Created(result);
            }
        )
        .WithTags(SwaggerTagsConstants.Transaction);
    }
}