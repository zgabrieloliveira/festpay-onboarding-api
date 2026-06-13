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

        dbContext.Transactions.Update(transaction);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class CancelTransactionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Transaction}/{{id:guid}}/cancel",
                async ([FromServices] ISender sender, [FromRoute] Guid id) =>
                {
                    var command = new CancelTransactionCommand(id);
                    var result = await sender.Send(command);
                    return Result.Ok(result);
                }
            )
            .WithTags("Transaction");
    }
}