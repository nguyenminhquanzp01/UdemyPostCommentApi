namespace Udemy.Application.Validators;

using FluentValidation;
using Udemy.Application.DTOs;

/// <summary>
/// Validator for create comment requests.
/// </summary>
public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateCommentRequestValidator"/> class.
    /// </summary>
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MinimumLength(1).WithMessage("Comment must not be empty.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}

/// <summary>
/// Validator for update comment requests.
/// </summary>
public class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateCommentRequestValidator"/> class.
    /// </summary>
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MinimumLength(1).WithMessage("Comment must not be empty.")
            .MaximumLength(2000).WithMessage("Comment must not exceed 2000 characters.");
    }
}
