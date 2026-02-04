using Microsoft.EntityFrameworkCore;
using MyGolfAPI.Data;
using MyGolfAPI.DTOs.Users;
using MyGolfAPI.Entities;
using System.Security.Claims;

namespace MyGolfAPI.Services.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly MyGolfDbContext _context;

        public CurrentUserService(MyGolfDbContext context)
        {
            _context = context;
        }

        private async Task<User> GetOrCreateUserEntityAsync(ClaimsPrincipal principal, CancellationToken ct)
        {
            var sub = principal.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("Missing 'sub' claim in token.");

            var email = principal.FindFirstValue("email") ?? "";
            var nickname = principal.FindFirstValue("nickname");
            var name = principal.FindFirstValue("name");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);

            if (user is null)
            {
                user = new User
                {
                    Auth0Sub = sub,
                    Email = email,
                    Username = !string.IsNullOrWhiteSpace(nickname) ? nickname :
                               !string.IsNullOrWhiteSpace(name) ? name :
                               (!string.IsNullOrWhiteSpace(email) ? email : "new-user@new.com")
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                // Keep email in sync (optional)
                if (!string.IsNullOrWhiteSpace(email) &&
                    !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    await _context.SaveChangesAsync(ct);
                }
            }

            return user;
        }

        private static UserReadDto ToReadDto(User user) => new()
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };


        public async Task<UserReadDto> GetMeAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        {
            var user = await GetOrCreateUserEntityAsync(principal, ct);
            return ToReadDto(user);
        }

        public async Task<UserReadDto> CreateOrCompleteMeAsync(ClaimsPrincipal principal, UserCreateDto dto, CancellationToken ct = default)
        {
            // Ensures row exists, then fills missing fields.
            var user = await GetOrCreateUserEntityAsync(principal, ct);

            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username.Trim();

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName.Trim();

            await _context.SaveChangesAsync(ct);
            return ToReadDto(user);
        }

        public async Task<UserReadDto> UpdateMeAsync(ClaimsPrincipal principal, UserUpdateDto dto, CancellationToken ct = default)
        {
            var sub = principal.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("Missing 'sub' claim in token.");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);
            if (user is null)
                throw new KeyNotFoundException("User profile not found.");

            // Patch semantics: only update non-null values
            if (dto.Username is not null)
                user.Username = dto.Username.Trim();

            if (dto.FirstName is not null)
                user.FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim();

            if (dto.LastName is not null)
                user.LastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim();

            await _context.SaveChangesAsync(ct);
            return ToReadDto(user);
        }

        public async Task DeleteMeAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        {
            var sub = principal.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("Missing 'sub' claim in token.");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);
            if (user is null)
                return; // idempotent delete

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
        }
    }
}
