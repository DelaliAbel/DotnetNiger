// Validation Community: UpdateCommentRequestValidator
using DotnetNiger.Community.Application.DTOs.Requests;
using DotnetNiger.Community.Application.Exceptions;

namespace DotnetNiger.Community.Application.Validators;

/// <summary>
/// Validation specifique pour mise a jour de comment.
/// </summary>
public static class UpdateCommentRequestValidator
{
    private const int MinContentLength = 1;
    private const int MaxContentLength = 5000;

    public static void ValidateAndThrow(UpdateCommentRequest request)
    {
        if (request == null)
            throw new NotFoundException("Update comment request is required.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new NotFoundException("Comment content cannot be empty.");

        if (request.Content.Length < MinContentLength)
            throw new NotFoundException($"Comment content must be at least {MinContentLength} character.");

        if (request.Content.Length > MaxContentLength)
            throw new NotFoundException($"Comment content must not exceed {MaxContentLength} characters.");
    }
}
