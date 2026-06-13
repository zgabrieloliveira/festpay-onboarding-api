using FluentValidation;
using MediatR;
using Festpay.Onboarding.Domain.Extensions;
using Festpay.Onboarding.Infra.Context;
using Festpay.Onboarding.Application.Common.Exceptions;
using Festpay.Onboarding.Domain.Entities;
using Carter;
using Microsoft.AspNetCore.Routing;
using Festpay.Onboarding.Application.Common.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Festpay.Onboarding.Application.Common.Models;
using Microsoft.AspNetCore.Http;

namespace Festpay.Onboarding.Application.Features.V1;

public sealed record CreateAccountCommand(
    string Name,
    string Document,
    string Email,
    string Phone
) : IRequest<bool>;

public sealed class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");

        RuleFor(x => x.Document)
            .NotEmpty()
            .WithMessage("Document is required.")
            .Must(x => x.IsValidDocument())
            .WithMessage("Invalid document number.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Invalid email format.");

        RuleFor(x => x.Phone)
            .Must(x => x.IsValidPhone())
            .When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage("Invalid phone number.");
    }
}

public sealed class CreateAccountCommandHandler(FestpayContext dbContext) : IRequestHandler<CreateAccountCommand, bool>
{
    public async Task<bool> Handle(
        CreateAccountCommand request,
        CancellationToken cancellationToken
    )
    {
        if (VerifyExistingAccount(request.Document))
        {
            throw new EntityAlreadyExistsException("Account");
        }

        var account = new Account.Builder()
            .WithName(request.Name)
            .WithDocument(request.Document)
            .WithEmail(request.Email)
            .WithPhone(request.Phone)
            .Build();

        await dbContext.Accounts.AddAsync(account, cancellationToken);
        return await dbContext.SaveChangesAsync(cancellationToken) > 0;
    }

    private bool VerifyExistingAccount(string document)
    {
        return dbContext.Accounts.Any(x => x.Document == document);
    }
}

public sealed class CreateAccountCommandEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost($"{EndpointConstants.V1}{EndpointConstants.Account}",
            async ([FromServices] ISender sender, [FromBody] CreateAccountCommand command) =>
            {
                var result = await sender.Send(command);
                return Result.Ok(result);
            }
        )
        .WithTags(SwaggerTagsConstants.Account);
    }
}
