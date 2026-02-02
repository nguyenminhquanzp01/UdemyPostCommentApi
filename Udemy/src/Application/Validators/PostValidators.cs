namespace Udemy.Application.Validators;

using FluentValidation;
using Udemy.Application.DTOs;

/// <summary>
/// Validator for create post requests.
/// </summary>
public class CreatePostRequestValidator : AbstractValidator<CreatePostRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreatePostRequestValidator"/> class.
    /// </summary>
    public CreatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.");
    }
}

/// <summary>
/// Validator for update post requests.
/// </summary>
public class UpdatePostRequestValidator : AbstractValidator<UpdatePostRequest>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdatePostRequestValidator"/> class.
    /// </summary>
    public UpdatePostRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(5).WithMessage("Title must be at least 5 characters.")
            .MaximumLength(500).WithMessage("Title must not exceed 500 characters.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters.");
    }
}
