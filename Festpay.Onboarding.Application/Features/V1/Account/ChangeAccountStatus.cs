using Carter;
using Festpay.Onboarding.Application.Common.Constants;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Application.Common.Models;
using Festpay.Onboarding.Domain.Entities;
using Festpay.Onboarding.Infra.Context;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Festpay.Onboarding.Application.Features.V1;

public sealed record ChangeAccountStatusCommand(Guid Id) : IRequest<bool>;

public sealed class ChangeAccountStatusCommandValidator
    : AbstractValidator<ChangeAccountStatusCommand>
{
    public ChangeAccountStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required.");
    }
}

public sealed class ChangeAccountStatusCommandHandler(FestpayContext dbContext)
    : IRequestHandler<ChangeAccountStatusCommand, bool>
{
    public async Task<bool> Handle(
        ChangeAccountStatusCommand request,
        CancellationToken cancellationToken
    )
    {
        var account = await dbContext.Accounts.FindAsync([request.Id], cancellationToken)
                      ?? throw new NotFoundException("Account");

        account.EnableDisable();
        dbContext.Accounts.Update(account);

        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }
}

public sealed class ChangeAccountStatusCommandEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch($"{EndpointConstants.V1}{EndpointConstants.Account}/{{id:guid}}",
            async ([FromServices] ISender sender, [FromRoute] Guid id) =>
            {
                var command = new ChangeAccountStatusCommand(id);
                var result = await sender.Send(command);
                return Result.Ok(result);
            }
        )
        .WithTags(SwaggerTagsConstants.Account);
    }
}
