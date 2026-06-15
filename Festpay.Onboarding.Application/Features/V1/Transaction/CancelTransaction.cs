using FluentValidation;
using MediatR;
using Festpay.Onboarding.Infra.Context;
using Festpay.Onboarding.Application.Common.Exceptions;
using Carter;
using Microsoft.AspNetCore.Routing;
using Festpay.Onboarding.Application.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Festpay.Onboarding.Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Festpay.Onboarding.Application.Features.V1.Transaction;

public sealed record CancelTransactionCommand(Guid Id) : IRequest<bool>;

public sealed class CancelTransactionCommandValidator : AbstractValidator<CancelTransactionCommand>
{
    public CancelTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Transaction Id is required.");
    }
}

public sealed class CancelTransactionCommandHandler(FestpayContext dbContext) 
    : IRequestHandler<CancelTransactionCommand, bool>
{
    public async Task<bool> Handle(CancelTransactionCommand request, CancellationToken cancellationToken)
    {
        var transaction = await dbContext.Transactions.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException("Transaction");

        transaction.Cancel();

        var sourceAccount = await dbContext.Accounts.FindAsync([transaction.SourceAccountId], cancellationToken)
            ?? throw new NotFoundException("Source Account");

        var destinationAccount = await dbContext.Accounts.FindAsync([transaction.DestinationAccountId], cancellationToken)
            ?? throw new NotFoundException("Destination Account");

        destinationAccount.Withdraw(transaction.Amount);
        sourceAccount.Deposit(transaction.Amount);

        dbContext.Transactions.Update(transaction);
        
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class CancelTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        /// <summary>
        /// Cancels an existing transaction and performs a financial reversal between accounts.
        /// </summary>
        /// <param name="id">The transaction ID to be cancelled.</param>
        /// <response code="200">Returns true if the cancellation was successful.</response>
        /// <response code="400">If the cancellation fails due to business rules (e.g., insufficient funds in the destination account).</response>
        /// <response code="404">If the transaction was not found.</response>
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Transaction}/{{id:guid}}/cancel",
                async ([FromServices] ISender sender, [FromRoute] Guid id) =>
                {
                    var command = new CancelTransactionCommand(id);
                    var result = await sender.Send(command);
                    return Result.Ok(result);
                }
            )
            .WithTags(SwaggerTagsConstants.Transaction);
    }
}