using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Infra.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Festpay.Onboarding.Application.Features.V1;

public sealed record DepositAccountCommand(Guid AccountId, decimal Amount) : IRequest<bool>;

public sealed class DepositAccountCommandValidator : AbstractValidator<DepositAccountCommand>
{
    public DepositAccountCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}

public sealed class DepositAccountCommandHandler(FestpayContext dbContext) 
    : IRequestHandler<DepositAccountCommand, bool>
{
    public async Task<bool> Handle(DepositAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await dbContext.Accounts.FindAsync([request.AccountId], cancellationToken)
                      ?? throw new NotFoundException("Account");

        account.Deposit(request.Amount);

        dbContext.Accounts.Update(account);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class DepositAccountEndpoint : ICarterModule
{
    /// <summary>
    /// Deposits a specified amount into an account.
    /// </summary>
    /// <param name="id">The account ID.</param>
    /// <param name="command">The deposit amount.</param>
    /// <response code="200">Returns true if the deposit was successful.</response>
    /// <response code="404">If the account was not found.</response>
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Account}/{{id:guid}}/deposit",
            async ([FromServices] ISender sender, [FromRoute] Guid id, [FromBody] DepositRequest body) =>
            {
                var command = new DepositAccountCommand(id, body.Amount);
                var result = await sender.Send(command);
                return Result.Ok(result);
            }
        )
        .WithTags(SwaggerTagsConstants.Account);
    }
}

public sealed record DepositRequest(decimal Amount);