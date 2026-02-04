using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyGolfAPI.Services.Auth;


namespace MyGolfAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public UsersController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var user = await _currentUserService.GetOrCreateAsync(User, ct);

            return Ok(new
            {
                user.Id,
                user.Auth0Sub,
                user.Username,
                user.Email,
                user.FirstName,
                user.LastName
            });
        }
    }
}
