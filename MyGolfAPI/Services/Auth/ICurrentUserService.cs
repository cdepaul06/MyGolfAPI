using MyGolfAPI.DTOs.Users;
using MyGolfAPI.Entities;
using System.Security.Claims;

namespace MyGolfAPI.Services.Auth
{
    public interface ICurrentUserService
    {
        Task<UserReadDto> GetMeAsync(ClaimsPrincipal principal, CancellationToken ct = default);
        Task<UserReadDto> CreateOrCompleteMeAsync(ClaimsPrincipal principal, UserCreateDto dto, CancellationToken ct = default);
        Task<UserReadDto> UpdateMeAsync(ClaimsPrincipal principal, UserUpdateDto dto, CancellationToken ct = default);
        Task DeleteMeAsync(ClaimsPrincipal principal, CancellationToken ct = default);
    }
}
