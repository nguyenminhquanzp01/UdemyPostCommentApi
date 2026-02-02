namespace Udemy.Infrastructure.Authorization;

using Microsoft.AspNetCore.Authorization;

/// <summary>
/// Authorization handler for owner-only operations (RBAC).
/// </summary>
public class OwnerAuthorizationHandler : AuthorizationHandler<OwnerRequirement, IOwnerResource>
{
    /// <summary>
    /// Handles authorization logic.
    /// </summary>
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerRequirement requirement,
        IOwnerResource resource)
    {
        var userIdClaim = context.User.FindFirst(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (Guid.TryParse(userIdClaim.Value, out var userId) && userId == resource.OwnerId)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorization requirement for owner-only access.
/// </summary>
public class OwnerRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Interface for resources that have an owner.
/// </summary>
public interface IOwnerResource
{
    /// <summary>
    /// Gets the owner ID of the resource.
    /// </summary>
    Guid OwnerId { get; }
}
