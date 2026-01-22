namespace Udemy.Application.Exceptions;

/// <summary>
/// Custom exception for application-level errors.
/// </summary>
public class ApplicationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationException"/> class.
    /// </summary>
    public ApplicationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationException"/> class.
    /// </summary>
    public ApplicationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
