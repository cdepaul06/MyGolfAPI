using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyGolfAPI.Data;

namespace MyGolfAPI.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly MyGolfDbContext _context;

        public TestController(MyGolfDbContext db) => _context = db;


        // GET https://localhost:7296/api/test/users
        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new { u.Id, u.Username, u.Email, u.FirstName, u.LastName })
                .ToListAsync();

            return Ok(users);

        }
    }
}
