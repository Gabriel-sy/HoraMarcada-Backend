using System.Security.Claims;

namespace Marque.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        if (principal == null)
        {
            throw new ArgumentNullException(nameof(principal));
        }

        var claimValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(claimValue))
        {
            throw new UnauthorizedAccessException("User identifier claim is missing.");
        }

        if (!Guid.TryParse(claimValue, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier claim is not a valid GUID.");
        }

        return userId;
    }
}
