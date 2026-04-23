using FluentValidation;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class ApproveMemberCommandValidator : AbstractValidator<ApproveMemberCommand>
{
    public ApproveMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.ReviewerUserId).NotEmpty();
    }
}
