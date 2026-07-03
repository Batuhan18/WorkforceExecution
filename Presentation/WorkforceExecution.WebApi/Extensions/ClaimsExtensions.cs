using System.Security.Claims;

namespace WorkforceExecution.WebApi.Extensions;

public static class ClaimsExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static int? GetCrewRegionId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("crewRegionId");
        return value is null ? null : int.Parse(value);
    }
}
