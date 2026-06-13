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
using Microsoft.EntityFrameworkCore;

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
        var sourceExists = await dbContext.Accounts.AnyAsync(a => a.Id == request.SourceAccountId, cancellationToken);
        if (!sourceExists)
            throw new NotFoundException("Source Account");

        var destinationExists = await dbContext.Accounts.AnyAsync(a => a.Id == request.DestinationAccountId, cancellationToken);
        if (!destinationExists)
            throw new NotFoundException("Destination Account");

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
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConstants.V1}{EndpointConstants.Transaction}",
            async ([FromServices] ISender sender, [FromBody] CreateTransactionCommand command) =>
            {
                var result = await sender.Send(command);
                return Result.Created(result);
            }
        )
        .WithTags("Transaction");
    }
}