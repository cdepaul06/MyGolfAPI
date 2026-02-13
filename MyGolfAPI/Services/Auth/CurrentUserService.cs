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

        #region INTERFACE METHODS

        private async Task<User> GetOrCreateUserEntityAsync(ClaimsPrincipal principal, CancellationToken ct)
        {
            var sub = GetSubOrThrow(principal);

            var email = principal.FindFirstValue("https://mygolfapi/email") ?? "";
            var name = principal.FindFirstValue("https://mygolfapi/name");
            var nickname = principal.FindFirstValue("https://mygolfapi/nickname");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);

            if (user is null)
            {        
               var baseUserName = !string.IsNullOrWhiteSpace(nickname) ? nickname :
                                   !string.IsNullOrWhiteSpace(name) ? name :
                                   (!string.IsNullOrWhiteSpace(email) ? email.Split('@')[0] : "golfer");

               var (username, normalized) = await GenerateUniqueUsernameAsync(baseUserName, ct);

                user = new User
                {
                    Auth0Sub = sub,
                    Email = email,
                    Username = username,
                    NormalizedUsername = normalized
                };

                _context.Users.Add(user);

                try
                {
                    await _context.SaveChangesAsync(ct);
                }
                catch (DbUpdateException ex)
                {
                    throw new InvalidOperationException("Failed to create user.", ex);
                }
            }
            else
            {
                // Keep email in sync
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
            NormalizedUsername = user.NormalizedUsername,
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
                await ApplyUsernameChangeAsync(user, dto.Username, ct);

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
                user.FirstName = dto.FirstName.Trim();

            if (!string.IsNullOrWhiteSpace(dto.LastName))
                user.LastName = dto.LastName.Trim();

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new ArgumentException("Username is already taken. Please choose another.", ex);
            }
             
            return ToReadDto(user);
        }

        public async Task<UserReadDto> UpdateMeAsync(ClaimsPrincipal principal, UserUpdateDto dto, CancellationToken ct = default)
        {
            var sub = GetSubOrThrow(principal);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct) ?? throw new KeyNotFoundException("User profile not found.");

            if (user is null)
                throw new KeyNotFoundException("User profile not found.");

            if (dto.Username is not null)
            {
                if (string.IsNullOrWhiteSpace(dto.Username))
                    throw new ArgumentException("Username cannot be empty.");
                await ApplyUsernameChangeAsync(user, dto.Username, ct);
            }


            if (dto.FirstName is not null)
                user.FirstName = string.IsNullOrWhiteSpace(dto.FirstName) ? null : dto.FirstName.Trim();

            if (dto.LastName is not null)
                user.LastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim();

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex)
            {
                throw new ArgumentException("Username is already taken. Please choose another.", ex);
            }
            
            return ToReadDto(user);
        }

        public async Task DeleteMeAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        {
            var sub = GetSubOrThrow(principal);

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);
            if (user is null)
                return; // idempotent delete

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);
        }

        private async Task ApplyUsernameChangeAsync(User user, string newUsername, CancellationToken ct)
        {
            ValidateUsernameOrThrow(newUsername);

            var normalized = NormalizeUsername(newUsername);

            if (string.Equals(user.NormalizedUsername, normalized, StringComparison.OrdinalIgnoreCase))
                return; // no change

            // Pre-check, DB constraint is final guard
            var taken = await _context.Users.AnyAsync(u => u.NormalizedUsername == normalized && u.Id != user.Id, ct);
            if (taken)
                throw new ArgumentException("Username is already taken. Please choose another.");

            user.Username = newUsername.Trim();
            user.NormalizedUsername = normalized;
        }

        #endregion

        #region HELPER METHODS

        private static string GetSubOrThrow(ClaimsPrincipal principal)
        {
            var sub = principal.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("Missing 'sub' claim in token.");
            return sub;
        }

        private static readonly HashSet<string> ReservedUsernames = new(StringComparer.OrdinalIgnoreCase)
        {
            "admin", "administrator", "support", "help", "root", "system",
            "owner", "moderator", "mod", "staff",
            "api", "auth", "login", "logout", "signup", "register",
            "mygolf", "null", "undefined"
        };

        private static bool IsReserved(string normalized) => ReservedUsernames.Contains(normalized);


        private static string NormalizeUsername(string input)
        {
            var u = input.Trim().ToLowerInvariant();

            u = u.Replace(' ', '_');

            return u;
        }

        private static void ValidateUsernameOrThrow(string rawUsername)
        {
            if (string.IsNullOrWhiteSpace(rawUsername))
                throw new ArgumentException("Username is required.");

            var normalized = NormalizeUsername(rawUsername);

            if (normalized.Length < 3 || normalized.Length > 50) 
                throw new ArgumentException("Username must be between 3 and 50 characters.");

            foreach (var ch in normalized)
            {
                var ok = char.IsLetterOrDigit(ch) || ch == '_' || ch == '-' || ch == '.';
                if (!ok)
                    throw new ArgumentException("Username may only contain letters, numbers, '-', '_' or '.'.");
            }

            if (IsReserved(normalized))
                throw new ArgumentException("This username is reserved. Please choose another.");
        }

        private async Task<(string Username, string Normalized)> GenerateUniqueUsernameAsync(string baseUsername, CancellationToken ct)
        {
            var normalizedBase = NormalizeUsername(baseUsername);

            if (IsReserved(normalizedBase))
                normalizedBase = "golfer"; // fallback base

            // if available, use it
            var exists = await _context.Users.AnyAsync(u => u.NormalizedUsername == normalizedBase, ct);
            if (!exists)
                return (normalizedBase, normalizedBase);

            for (var i = 1; i < 10_000; i++)
            {
                var candidateNorm = $"{normalizedBase}{i}";
                if (IsReserved(candidateNorm)) continue;

                var taken = await _context.Users.AnyAsync(u => u.NormalizedUsername == candidateNorm, ct);
                if (!taken)
                    return (candidateNorm, candidateNorm);
            }

            throw new InvalidOperationException("Unable to generate a unique username.");
        }
        #endregion
    }
}
