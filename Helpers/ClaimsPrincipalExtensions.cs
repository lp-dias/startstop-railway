using System.Security.Claims;

namespace StartStop.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasRole(this ClaimsPrincipal user, string role)
        {
            return user.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role);
        }
    }
}
