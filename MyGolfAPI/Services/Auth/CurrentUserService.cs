using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyGolfAPI.Data;
using MyGolfAPI.Entities;

namespace MyGolfAPI.Services.Auth
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly MyGolfDbContext _context;

        public CurrentUserService(MyGolfDbContext context)
        {
            _context = context;
        }

        public async Task<User> GetOrCreateAsync(ClaimsPrincipal principal, CancellationToken ct = default)
        {
            var sub = principal.FindFirstValue("sub");
            if (string.IsNullOrWhiteSpace(sub))
                throw new InvalidOperationException("The 'sub' claim is missing from token.");

            var email = principal.FindFirstValue("email") ?? "";
            var name = principal.FindFirstValue("name");
            var nickname = principal.FindFirstValue("nickname");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Auth0Sub == sub, ct);

            if (user is null)
            {
                user = new User
                {
                    Auth0Sub = sub,
                    Email = email,
                    Username = !string.IsNullOrWhiteSpace(nickname) ? nickname : (!string.IsNullOrWhiteSpace(name) ? name : email),
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync(ct);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(email) && !string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    user.Email = email;
                    await _context.SaveChangesAsync(ct);
                }
            }

            return user;
        }
    }
}
