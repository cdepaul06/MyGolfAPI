using MyGolfAPI.Entities;
using System.Security.Claims;

namespace MyGolfAPI.Services.Auth
{
    public interface ICurrentUserService
    {
        Task<User> GetOrCreateAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    }
}
