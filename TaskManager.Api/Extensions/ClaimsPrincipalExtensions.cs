using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TaskManager.Api.Entity;

namespace TaskManager.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new InvalidOperationException("The authorized user has a null ID.");
    }
}
