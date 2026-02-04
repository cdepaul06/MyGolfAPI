using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyGolfAPI.Services.Auth;


namespace MyGolfAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public UsersController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        // Read - Get current user info
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var dto = await _currentUserService.GetMeAsync(User, ct);
            return Ok(dto);
        }

        // Complete - Create or complete current user info
        [HttpPost("me")]
        public async Task<IActionResult> CreateOrCompleteMe([FromBody] DTOs.Users.UserCreateDto dto, CancellationToken ct)
        {
            var result = await _currentUserService.CreateOrCompleteMeAsync(User, dto, ct);
            return Ok(result);
        }

        // Update - Update current user info
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateMe([FromBody] DTOs.Users.UserUpdateDto dto, CancellationToken ct)
        {
            var result = await _currentUserService.UpdateMeAsync(User, dto, ct);
            return Ok(result);
        }

        // Delete - Delete current user (Local DB row only)
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteMe(CancellationToken ct)
        {
            await _currentUserService.DeleteMeAsync(User, ct);
            return NoContent();

        }
    }
}
