using FluentValidation;

namespace DotnetNiger.Community.Application.Features.FeatureSettings.Commands;

public sealed class UpdateCommunityFeatureSettingsCommandValidator : AbstractValidator<UpdateCommunityFeatureSettingsCommand>
{
    public UpdateCommunityFeatureSettingsCommandValidator()
    {
        RuleFor(command => command.Request).NotNull();
    }
}
