using FluentValidation;

namespace DotnetNiger.Community.Application.Features.Members.Commands;

public sealed class RejectMemberCommandValidator : AbstractValidator<RejectMemberCommand>
{
    public RejectMemberCommandValidator()
    {
        RuleFor(x => x.MemberId).NotEmpty();
        RuleFor(x => x.ReviewerUserId).NotEmpty();
    }
}
