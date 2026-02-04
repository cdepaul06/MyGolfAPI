using Microsoft.EntityFrameworkCore;
using MyGolfAPI.Data;
using MyGolfAPI.DTOs.Users;

namespace MyGolfAPI.Services.Users
{
    public class UserService : IUserService
    {
        private readonly MyGolfDbContext _context;

        public UserService(MyGolfDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserReadDto>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Select(u => new UserReadDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync(ct);
        }
    }
}
